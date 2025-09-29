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
    public class FuncionarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FuncionarioController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            await DesactivarReservasExpiradas();
            var model = new DashboardFuncionarioViewModel();

            // Traer los parqueos activos con la info de vehículo y usuario
            var vehiculos = await _context.Parqueos
                .Include(p => p.Vehiculo)
                .Include(p => p.Usuario)
                .Where(p => p.FechaSalida == null) // Solo los que están dentro
                .ToListAsync();

            // Mapear a VehiculoActivoDto
            model.VehiculosActivos = vehiculos.Select(p => new VehiculoActivoDto
            {
                Id = p.Id,
                Placa = p.Vehiculo.Placa,
                Tipo = p.Vehiculo.Tipo.ToString(),
                NombrePropietario = p.Usuario.Nombre,
                HoraEntrada = p.FechaEntrada,
                TiempoTranscurrido = CalcularTiempoTranscurrido(p.FechaEntrada),
                Estado = p.Estado
            }).ToList();

            // Calcular métricas adicionales del dashboard
            model.CarrosDentro = vehiculos.Count(v => v.Vehiculo.Tipo == TipoVehiculo.Carro);
            model.MotasDentro = vehiculos.Count(v => v.Vehiculo.Tipo == TipoVehiculo.Moto);

            var hoy = DateTime.Now.Date;
            model.IngresosHoy = await _context.Parqueos
                .Where(p => p.FechaEntrada.Date == hoy && p.FechaSalida != null)
                .SumAsync(p => (decimal)p.TotalPagar);

            model.ReservasActivas = await _context.Reservas
                .CountAsync(r => r.Activa && r.ExpiraEn > DateTime.Now);

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> RegistrarIngreso(string placa)
        {
            try
            {
                await DesactivarReservasExpiradas();

                var vehiculo = await _context.Vehiculos
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync(v => v.Placa.ToLower() == placa.ToLower());

                if (vehiculo == null)
                {
                    return Json(new { success = false, message = "Vehículo no registrado" });
                }

                if (await _context.Parqueos.AnyAsync(p => p.VehiculoId == vehiculo.Id && p.FechaSalida == null))
                {
                    return Json(new { success = false, message = "El vehículo ya está en el parqueadero" });
                }

                var reserva = await _context.Reservas
                    .FirstOrDefaultAsync(r => r.VehiculoId == vehiculo.Id && r.Activa);

                string estado;
                if (reserva != null)
                {
                    estado = "Dentro";
                    reserva.Activa = false; // Usar la reserva
                }
                else
                {
                    var cuposOcupados = await _context.Parqueos
                        .CountAsync(p => p.Vehiculo.Tipo == vehiculo.Tipo && p.Estado == "Dentro");
                    
                    int cuposMaximos = vehiculo.Tipo == TipoVehiculo.Carro ? 20 : 20;
                    estado = cuposOcupados >= cuposMaximos ? "Fuera" : "Dentro";
                }

                var parqueo = new Parqueos
                {
                    VehiculoId = vehiculo.Id,
                    UsuarioId = vehiculo.UsuarioId,
                    FechaEntrada = DateTime.Now,
                    Estado = estado,
                    TotalPagar = 0,
                    Vehiculo = vehiculo,
                    Usuario = vehiculo.Usuario
                };

                _context.Parqueos.Add(parqueo);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Ingreso registrado ({estado}) para {vehiculo.Placa}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarSalida(int parqueoId)
        {
            try
            {
                var parqueo = await _context.Parqueos
                    .Include(p => p.Vehiculo)
                    .Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.Id == parqueoId);

                if (parqueo == null)
                {
                    return Json(new { success = false, message = "Registro de parqueo no encontrado" });
                }

                if (parqueo.FechaSalida != null)
                {
                    return Json(new { success = false, message = "Este vehículo ya registró su salida" });
                }

                // Calcular el tiempo y costo
                parqueo.FechaSalida = DateTime.Now;
                var tiempoTotal = parqueo.FechaSalida.Value - parqueo.FechaEntrada;
                
                // Obtener tarifa (valor por hora)
                var tarifa = await _context.Tarifas
                    .FirstOrDefaultAsync(t => t.TipoVehiculo == parqueo.Vehiculo.Tipo.ToString() && t.Ubicacion == parqueo.Estado);
                
                decimal valorHora = tarifa?.ValorHora != null 
                    ? (decimal)tarifa.ValorHora 
                    : (parqueo.Vehiculo.Tipo == TipoVehiculo.Carro ? 2000m : 1500m); // Fallback por si no se encuentra tarifa
                
                // Calcular cobro (mínimo 1 hora)
                double horasCobrar = Math.Max(1, Math.Ceiling(tiempoTotal.TotalHours));
                parqueo.TotalPagar = (double)(horasCobrar * (double)valorHora);
                parqueo.Estado = "Finalizado";

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Salida registrada exitosamente. Total: ${parqueo.TotalPagar:N0}",
                    total = parqueo.TotalPagar,
                    tiempo = FormatearTiempo(tiempoTotal)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarVehiculo(string placa)
        {
            try
            {
                var vehiculo = await _context.Vehiculos
                    .Include(v => v.Usuario)
                    .FirstOrDefaultAsync(v => v.Placa.ToLower().Contains(placa.ToLower()));

                if (vehiculo == null)
                {
                    return Json(new { success = false, message = "Vehículo no encontrado" });
                }

                // Obtener rol del usuario
                var roles = await _userManager.GetRolesAsync(vehiculo.Usuario);
                var rol = roles.FirstOrDefault() ?? "Sin rol";

                return Json(new { 
                    success = true, 
                    vehiculo = new {
                        placa = vehiculo.Placa,
                        tipo = vehiculo.Tipo,
                        propietario = vehiculo.Usuario.Nombre,
                        email = vehiculo.Usuario.Email,
                        rol = rol
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ActualizarDashboard()
        {
            // Método para actualización AJAX del dashboard
            var vehiculosDentro = await _context.Parqueos
                .Include(p => p.Vehiculo)
                .Where(p => p.FechaSalida == null)
                .ToListAsync();

            var carrosDentro = vehiculosDentro.Count(v => v.Vehiculo.Tipo == TipoVehiculo.Carro);
            var motasDentro = vehiculosDentro.Count(v => v.Vehiculo.Tipo == TipoVehiculo.Moto);

            var hoy = DateTime.Now.Date;
            var ingresosDiarios = await _context.Parqueos
                .Where(p => p.FechaEntrada.Date == hoy && p.FechaSalida != null)
                .SumAsync(p => (decimal)p.TotalPagar);

            var reservasActivas = await _context.Reservas
                .CountAsync(r => r.Activa && r.ExpiraEn > DateTime.Now);

            return Json(new {
                carrosDentro,
                motasDentro,
                ingresosHoy = ingresosDiarios,
                reservasActivas
            });
        }

        private string CalcularTiempoTranscurrido(DateTime entrada)
        {

            var tiempo = DateTime.Now - entrada;
            if (tiempo.TotalHours >= 1)
                return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
            else
                return $"{(int)tiempo.TotalMinutes}m";
        }

        private string FormatearTiempo(TimeSpan tiempo)
        {
            if (tiempo.TotalHours >= 1)
                return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
            else
                return $"{(int)tiempo.TotalMinutes}m";
        }

        private async Task DesactivarReservasExpiradas()
        {
            var ahora = DateTime.Now;
            var reservasExpiradas = await _context.Reservas
                .Where(r => r.Activa && r.ExpiraEn <= ahora)
                .ToListAsync();

            if (reservasExpiradas.Any())
            {
                foreach (var reserva in reservasExpiradas)
                {
                    reserva.Activa = false;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}