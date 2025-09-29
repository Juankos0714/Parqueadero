using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ReportesViewModel();

            // Fecha por defecto: último mes
            var fechaInicio = DateTime.Now.AddDays(-30).Date;
            var fechaFin = DateTime.Now.Date.AddDays(1);

            // Calcular métricas principales
            var parqueosCompletados = await _context.Parqueos
                .Include(p => p.Vehiculo)
                .Include(p => p.Usuario)
                .Where(p => p.FechaEntrada >= fechaInicio && 
                           p.FechaEntrada < fechaFin && 
                           p.FechaSalida != null)
                .ToListAsync();

            model.IngresosTotales = (decimal)parqueosCompletados.Sum(p => p.TotalPagar);
            model.VehiculosAtendidos = parqueosCompletados.Count;
            model.CarrosAtendidos = parqueosCompletados.Count(p => p.Vehiculo.Tipo == "Carro");
            model.MotosAtendidas = parqueosCompletados.Count(p => p.Vehiculo.Tipo == "Moto");

            // Calcular tiempo promedio
            if (parqueosCompletados.Any())
            {
                var tiempoPromedioMinutos = parqueosCompletados
                    .Where(p => p.FechaSalida.HasValue)
                    .Average(p => (p.FechaSalida.Value - p.FechaEntrada).TotalMinutes);
                
                var horas = (int)(tiempoPromedioMinutos / 60);
                var minutos = (int)(tiempoPromedioMinutos % 60);
                model.TiempoPromedio = $"{horas}h {minutos}m";
            }
            else
            {
                model.TiempoPromedio = "0h 0m";
            }

            // Calcular ocupación promedio (simulado)
            model.OcupacionPromedio = 65; // Esto se podría calcular con datos históricos reales

            // Historial detallado
            model.HistorialDetallado = await GetHistorialDetallado(fechaInicio, fechaFin);

            // Ingresos diarios para gráfico
            model.IngresosDiarios = await GetIngresosDiarios(fechaInicio, fechaFin);

            // Pagos por vehículo
            model.PagosPorVehiculo = await GetPagosPorVehiculo(fechaInicio, fechaFin);

            var parqueosActivos = await _context.Parqueos
                .Include(p => p.Vehiculo)
                .Where(p => p.FechaSalida == null)
                .ToListAsync();

            model.CarrosDentro = parqueosActivos.Count(p => p.Vehiculo.Tipo == "Carro" && p.Estado == "Dentro");
            model.MotosDentro = parqueosActivos.Count(p => p.Vehiculo.Tipo == "Moto" && p.Estado == "Dentro");
            model.CarrosFuera = parqueosActivos.Count(p => p.Vehiculo.Tipo == "Carro" && p.Estado == "Fuera");
            model.MotosFuera = parqueosActivos.Count(p => p.Vehiculo.Tipo == "Moto" && p.Estado == "Fuera");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FiltrarReportes(DateTime? fechaInicio, DateTime? fechaFin, string? tipoVehiculo)
        {
            // Validar fechas
            fechaInicio ??= DateTime.Now.AddDays(-30).Date;
            fechaFin ??= DateTime.Now.Date.AddDays(1);

            var query = _context.Parqueos
                .Include(p => p.Vehiculo)
                .Include(p => p.Usuario)
                .Where(p => p.FechaEntrada >= fechaInicio && 
                           p.FechaEntrada < fechaFin && 
                           p.FechaSalida != null);

            if (!string.IsNullOrEmpty(tipoVehiculo) && tipoVehiculo != "todos")
            {
                var tipo = tipoVehiculo == "carro" ? "Carro" : "Moto";
                query = query.Where(p => p.Vehiculo.Tipo == tipo);
            }

            var parqueosCompletados = await query.ToListAsync();

            var result = new
            {
                ingresosTotales = (decimal)parqueosCompletados.Sum(p => p.TotalPagar),
                vehiculosAtendidos = parqueosCompletados.Count,
                carrosAtendidos = parqueosCompletados.Count(p => p.Vehiculo.Tipo == "Carro"),
                motosAtendidas = parqueosCompletados.Count(p => p.Vehiculo.Tipo == "Moto"),
                tiempoPromedio = CalcularTiempoPromedio(parqueosCompletados),
                ocupacionPromedio = 65, // Simulado
                historialDetallado = await GetHistorialDetallado(fechaInicio.Value, fechaFin.Value, tipoVehiculo),
                ingresosDiarios = await GetIngresosDiarios(fechaInicio.Value, fechaFin.Value, tipoVehiculo),
                pagosPorVehiculo = await GetPagosPorVehiculo(fechaInicio.Value, fechaFin.Value, tipoVehiculo)
            };

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel(DateTime? fechaInicio, DateTime? fechaFin, string? tipoVehiculo)
        {
            try
            {
                // Aquí se implementaría la exportación a Excel
                // Por ahora, simulamos la descarga
                
                fechaInicio ??= DateTime.Now.AddDays(-30).Date;
                fechaFin ??= DateTime.Now.Date.AddDays(1);

                var historial = await GetHistorialDetallado(fechaInicio.Value, fechaFin.Value, tipoVehiculo);
                
                // Crear contenido CSV simple como ejemplo
                var csvContent = "Fecha,Placa,Tipo,Propietario,Rol,Entrada,Salida,Tiempo,Tarifa,Total\n";
                
                foreach (var registro in historial)
                {
                    csvContent += $"{registro.Fecha:dd/MM/yyyy}," +
                                 $"{registro.Placa}," +
                                 $"{registro.TipoVehiculo}," +
                                 $"{registro.NombrePropietario}," +
                                 $"{registro.RolUsuario}," +
                                 $"{registro.HoraEntrada}," +
                                 $"{registro.HoraSalida}," +
                                 $"{registro.TiempoTotal}," +
                                 $"{registro.TarifaHora}," +
                                 $"{registro.TotalPagado}\n";
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                var fileName = $"Reporte_Parqueadero_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al generar el archivo Excel" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportarPDF(DateTime? fechaInicio, DateTime? fechaFin, string? tipoVehiculo)
        {
            try
            {
                // Aquí se implementaría la exportación a PDF
                // Por ahora, retornamos un mensaje de éxito
                
                return Json(new { 
                    success = true, 
                    message = "El reporte PDF se generará en breve. Esta funcionalidad requiere una librería de PDF como iTextSharp o similares." 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al generar el archivo PDF" });
            }
        }

        private async Task<List<HistorialDetalladoDto>> GetHistorialDetallado(DateTime fechaInicio, DateTime fechaFin, string? tipoVehiculo = null)
        {
            var query = _context.Parqueos
                .Include(p => p.Vehiculo)
                .Include(p => p.Usuario)
                .Where(p => p.FechaEntrada >= fechaInicio && 
                           p.FechaEntrada < fechaFin && 
                           p.FechaSalida != null);

            if (!string.IsNullOrEmpty(tipoVehiculo) && tipoVehiculo != "todos")
            {
                var tipo = tipoVehiculo == "carro" ? "Carro" : "Moto";
                query = query.Where(p => p.Vehiculo.Tipo == tipo);
            }

            var parqueos = await query
                .OrderByDescending(p => p.FechaEntrada)
                .Take(100) // Limitar a 100 registros para rendimiento
                .ToListAsync();

            var resultado = new List<HistorialDetalladoDto>();
            var tarifas = await _context.Tarifas.ToListAsync();

            foreach (var parqueo in parqueos)
            {
                var roles = await _userManager.GetRolesAsync(parqueo.Usuario);
                var rol = roles.FirstOrDefault() ?? "Sin rol";
                var tarifa = tarifas.FirstOrDefault(t => t.TipoVehiculo == parqueo.Vehiculo.Tipo && t.Ubicacion == parqueo.Estado);

                resultado.Add(new HistorialDetalladoDto
                {
                    Fecha = parqueo.FechaEntrada.Date,
                    Placa = parqueo.Vehiculo.Placa,
                    TipoVehiculo = parqueo.Vehiculo.Tipo,
                    NombrePropietario = parqueo.Usuario.Nombre,
                    RolUsuario = rol,
                    HoraEntrada = parqueo.FechaEntrada.ToString("HH:mm"),
                    HoraSalida = parqueo.FechaSalida?.ToString("HH:mm") ?? "",
                    TiempoTotal = CalcularTiempoTotal(parqueo.FechaEntrada, parqueo.FechaSalida),
                    TarifaHora = (decimal)(tarifa?.ValorHora ?? 0),
                    TotalPagado = (decimal)parqueo.TotalPagar
                });
            }

            return resultado;
        }

        private async Task<List<IngresoDiarioDto>> GetIngresosDiarios(DateTime fechaInicio, DateTime fechaFin, string? tipoVehiculo = null)
        {
            var query = _context.Parqueos
                .Include(p => p.Vehiculo)
                .Where(p => p.FechaEntrada >= fechaInicio && 
                           p.FechaEntrada < fechaFin && 
                           p.FechaSalida != null);

            if (!string.IsNullOrEmpty(tipoVehiculo) && tipoVehiculo != "todos")
            {
                var tipo = tipoVehiculo == "carro" ? "Carro" : "Moto";
                query = query.Where(p => p.Vehiculo.Tipo == tipo);
            }

            var ingresosPorDia = await query
                .GroupBy(p => p.FechaEntrada.Date)
                .Select(g => new IngresoDiarioDto
                {
                    Fecha = g.Key,
                    TotalIngresos = (decimal)g.Sum(p => p.TotalPagar),
                    CantidadVehiculos = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            return ingresosPorDia;
        }

        private string CalcularTiempoPromedio(List<Parqueos> parqueos)
        {
            if (!parqueos.Any())
                return "0h 0m";

            var tiempoPromedioMinutos = parqueos
                .Where(p => p.FechaSalida.HasValue)
                .Average(p => (p.FechaSalida.Value - p.FechaEntrada).TotalMinutes);

            var horas = (int)(tiempoPromedioMinutos / 60);
            var minutos = (int)(tiempoPromedioMinutos % 60);
            return $"{horas}h {minutos}m";
        }

        private string CalcularTiempoTotal(DateTime entrada, DateTime? salida)
        {
            if (!salida.HasValue)
                return "En progreso";

            var tiempo = salida.Value - entrada;
            if (tiempo.TotalHours >= 1)
                return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
            else
                return $"{(int)tiempo.TotalMinutes}m";
        }

        private async Task<List<PagoPorVehiculoDto>> GetPagosPorVehiculo(DateTime fechaInicio, DateTime fechaFin, string? tipoVehiculo = null)
        {
            var query = _context.Parqueos
                .Include(p => p.Vehiculo)
                .ThenInclude(v => v.Usuario)
                .Where(p => p.FechaEntrada >= fechaInicio &&
                           p.FechaEntrada < fechaFin &&
                           p.FechaSalida != null);

            if (!string.IsNullOrEmpty(tipoVehiculo) && tipoVehiculo != "todos")
            {
                var tipo = tipoVehiculo == "carro" ? "Carro" : "Moto";
                query = query.Where(p => p.Vehiculo.Tipo == tipo);
            }

            var pagos = await query
                .GroupBy(p => new { p.Vehiculo.Placa, p.Vehiculo.Tipo, p.Vehiculo.Usuario.Nombre })
                .Select(g => new PagoPorVehiculoDto
                {
                    Placa = g.Key.Placa,
                    TipoVehiculo = g.Key.Tipo,
                    NombrePropietario = g.Key.Nombre,
                    TotalPagado = (decimal)g.Sum(p => p.TotalPagar)
                })
                .OrderByDescending(p => p.TotalPagado)
                .ToListAsync();

            return pagos;
        }
    }
}