// Forms and UI Functionality

// Notifications System
const notifications = {
    success: (message) => {
        if (typeof toastr !== 'undefined') {
            toastr.success(message, 'Éxito', {
                timeOut: 3000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    },
    
    warning: (message) => {
        if (typeof toastr !== 'undefined') {
            toastr.warning(message, 'Atención', {
                timeOut: 5000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    },
    
    error: (message) => {
        if (typeof toastr !== 'undefined') {
            toastr.error(message, 'Error', {
                timeOut: 7000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    },
    
    info: (message) => {
        if (typeof toastr !== 'undefined') {
            toastr.info(message, 'Información', {
                timeOut: 4000,
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right'
            });
        }
    }
};

// Debounce function for search inputs
function debounce(func, wait, immediate) {
    var timeout;
    return function() {
        var context = this, args = arguments;
        var later = function() {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
}

// Modal Management
function openModal(modalId) {
    $(`#${modalId}`).modal('show');
}

function closeModal(modalId) {
    $(`#${modalId}`).modal('hide');
}

// Loading States
function setLoadingState(buttonId, isLoading) {
    const button = $(`#${buttonId}`);
    if (isLoading) {
        button.addClass('loading').prop('disabled', true);
        button.find('.spinner').show();
    } else {
        button.removeClass('loading').prop('disabled', false);
        button.find('.spinner').hide();
    }
}

// AJAX Error Handler
function handleAjaxError(xhr, status, error) {
    console.error('AJAX Error:', status, error);
    notifications.error('Error de conexión. Por favor, intenta nuevamente.');
}

// Form Validation Helpers
function validateRequired(input, message = 'Este campo es requerido') {
    const value = $(input).val().trim();
    const formGroup = $(input).closest('.form-group');
    
    if (!value) {
        $(input).addClass('is-invalid').removeClass('is-valid');
        formGroup.addClass('invalid').removeClass('valid');
        showFieldError(input, message);
        return false;
    } else {
        $(input).addClass('is-valid').removeClass('is-invalid');
        formGroup.addClass('valid').removeClass('invalid');
        hideFieldError(input);
        return true;
    }
}

function validateEmail(input) {
    const email = $(input).val().trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const formGroup = $(input).closest('.form-group');
    
    if (!email) {
        return validateRequired(input, 'El email es requerido');
    } else if (!emailRegex.test(email)) {
        $(input).addClass('is-invalid').removeClass('is-valid');
        formGroup.addClass('invalid').removeClass('valid');
        showFieldError(input, 'Formato de email inválido');
        return false;
    } else {
        $(input).addClass('is-valid').removeClass('is-invalid');
        formGroup.addClass('valid').removeClass('invalid');
        hideFieldError(input);
        return true;
    }
}

function showFieldError(input, message) {
    const errorElement = $(input).siblings('.field-validation-error');
    if (errorElement.length) {
        errorElement.text(message).show();
    } else {
        $(input).after(`<span class="field-validation-error">${message}</span>`);
    }
}

function hideFieldError(input) {
    $(input).siblings('.field-validation-error').hide();
}

// Vehicle Search
function searchVehicleByPlaca(placa) {
    if (placa.length < 3) {
        $('#searchResults').hide().empty();
        return;
    }

    $.ajax({
        url: '/Funcionario/BuscarVehiculo',
        type: 'GET',
        data: { placa: placa },
        success: function(response) {
            if (response.success) {
                showVehicleInfo(response.vehiculo);
            } else {
                hideVehicleInfo();
                notifications.warning('Vehículo no encontrado');
            }
        },
        error: handleAjaxError
    });
}

function showVehicleInfo(vehiculo) {
    $('#vehicleInfo').show();
    $('#ownerName').text(vehiculo.propietario);
    $('#vehicleType').text(vehiculo.tipo);
    $('#userRole').text(vehiculo.rol);
    
    // Check availability
    checkAvailability(vehiculo.tipo);
}

function hideVehicleInfo() {
    $('#vehicleInfo').hide();
    $('#availabilityCheck').hide();
    $('#confirmarIngreso').prop('disabled', true);
}

function checkAvailability(tipoVehiculo) {
    // This would be replaced with actual availability check
    $('#availabilityCheck').show();
    $('#availabilityCheck .alert')
        .removeClass('alert-info alert-success alert-danger')
        .addClass('alert-success')
        .html('<i class="fas fa-check-circle"></i> Cupos disponibles para ' + tipoVehiculo.toLowerCase() + 's');
    
    $('#confirmarIngreso').prop('disabled', false);
}

// Parking Entry/Exit Management
function registrarIngreso() {
    const placa = $('#placaIngreso').val().trim();
    
    if (!placa) {
        notifications.error('Ingresa la placa del vehículo');
        return;
    }

    setLoadingState('confirmarIngreso', true);

    $.ajax({
        url: '/Funcionario/RegistrarIngreso',
        type: 'POST',
        data: { placa: placa },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                notifications.success(response.message);
                closeModal('ingresoModal');
                resetIngresoForm();
                refreshDashboard();
            } else {
                notifications.error(response.message);
            }
        },
        error: handleAjaxError,
        complete: function() {
            setLoadingState('confirmarIngreso', false);
        }
    });
}

function registrarSalida(parqueoId) {
    if (!parqueoId) {
        const selectedVehicle = $('#vehiculoSalida').val();
        if (!selectedVehicle) {
            notifications.error('Selecciona un vehículo');
            return;
        }
        parqueoId = selectedVehicle;
    }

    setLoadingState('confirmarSalida', true);

    $.ajax({
        url: '/Funcionario/RegistrarSalida',
        type: 'POST',
        data: { parqueoId: parqueoId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                notifications.success(response.message);
                closeModal('salidaModal');
                resetSalidaForm();
                refreshDashboard();
            } else {
                notifications.error(response.message);
            }
        },
        error: handleAjaxError,
        complete: function() {
            setLoadingState('confirmarSalida', false);
        }
    });
}

function calculateParkingFee(parqueoId) {
    // This would calculate the parking fee based on time
    // For now, we'll simulate the calculation
    const entryTime = $(`option[value="${parqueoId}"]`).data('entry');
    const currentTime = new Date();
    const entry = new Date(entryTime);
    
    const diffMs = currentTime - entry;
    const diffHours = Math.ceil(diffMs / (1000 * 60 * 60));
    const fee = diffHours * 2000; // Assuming 2000 per hour
    
    $('#calculationPanel').show();
    $('#horaEntrada').text(entry.toLocaleString());
    $('#horaSalida').text(currentTime.toLocaleString());
    $('#tiempoTotal').text(`${diffHours} hora(s)`);
    $('#totalPagar').text(`$${fee.toLocaleString()}`);
    $('#tarifaAplicada').text('$2,000/hora');
    
    $('#confirmarSalida').prop('disabled', false);
}

// Form Reset Functions
function resetIngresoForm() {
    $('#placaIngreso').val('');
    $('#searchResults').hide().empty();
    $('#vehicleInfo').hide();
    $('#availabilityCheck').hide();
    $('#confirmarIngreso').prop('disabled', true);
}

function resetSalidaForm() {
    $('#vehiculoSalida').val('');
    $('#calculationPanel').hide();
    $('#confirmarSalida').prop('disabled', true);
}

// Dashboard Refresh
function refreshDashboard() {
    if (typeof updateDashboardMetrics === 'function') {
        updateDashboardMetrics();
    }
    if (typeof updateVehicleTable === 'function') {
        updateVehicleTable();
    }
    notifications.info('Dashboard actualizado');
}

// Time Counter Updates
function updateTimeCounters() {
    $('.time-counter').each(function() {
        const entryTime = $(this).data('entry');
        if (entryTime) {
            const entry = new Date(entryTime);
            const now = new Date();
            const diff = now - entry;
            const hours = Math.floor(diff / (1000 * 60 * 60));
            const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            
            $(this).find('.badge').text(`${hours}h ${minutes}m`);
        }
    });
}

// Export Functions
function exportToExcel() {
    notifications.info('Preparando archivo Excel...');
    // This would trigger Excel export
    window.location.href = '/Reportes/ExportarExcel';
}

function exportToPDF() {
    notifications.info('Preparando archivo PDF...');
    // This would trigger PDF export
    window.location.href = '/Reportes/ExportarPDF';
}

// Initialize when document is ready
$(document).ready(function() {
    // Configure toastr if available
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            newestOnTop: true,
            preventDuplicates: false,
            onclick: null,
            showDuration: "300",
            hideDuration: "1000",
            timeOut: "5000",
            extendedTimeOut: "1000",
            showEasing: "swing",
            hideEasing: "linear",
            showMethod: "fadeIn",
            hideMethod: "fadeOut"
        };
    }

    // Auto-focus first form input
    $('form input:not([type="hidden"]):first').focus();

    // Enter key submission for forms
    $('form').on('keypress', function(e) {
        if (e.which === 13 && !$(e.target).is('textarea')) {
            e.preventDefault();
            $(this).find('button[type="submit"]:first').click();
        }
    });

    // Update time counters every 30 seconds
    setInterval(updateTimeCounters, 30000);
    updateTimeCounters(); // Initial update
});

// Global error handler for unhandled promise rejections
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    notifications.error('Ha ocurrido un error inesperado. Por favor, recarga la página.');
});

// Export functions to global scope
window.notifications = notifications;
window.openModal = openModal;
window.closeModal = closeModal;
window.registrarIngreso = registrarIngreso;
window.registrarSalida = registrarSalida;
window.refreshDashboard = refreshDashboard;
window.exportToExcel = exportToExcel;
window.exportToPDF = exportToPDF;
window.searchVehicleByPlaca = searchVehicleByPlaca;
window.calculateParkingFee = calculateParkingFee;