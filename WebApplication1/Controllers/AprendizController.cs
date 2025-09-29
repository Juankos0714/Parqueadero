using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Aprendiz")]
    public class AprendizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AprendizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            await DesactivarReservasExpiradas();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DashboardAprendizViewModel
            {
                NombreUsuario = user.Nombre,
                CantidadVehiculos = await _context.Vehiculos.CountAsync(v => v.UsuarioId == user.Id)
            };

            // Obtener reserva activa
            var reservaActiva = await _context.Reservas
                .Include(r => r.Vehiculo)
                .Where(r => r.UsuarioId == user.Id && r.Activa && r.ExpiraEn > DateTime.Now)
                .FirstOrDefaultAsync();

            if (reservaActiva != null)
            {
                model.ReservaActiva = new ReservaActivaDto
                {
                    Id = reservaActiva.Id,
                    PlacaVehiculo = reservaActiva.Vehiculo.Placa,
                    ExpiraEn = reservaActiva.ExpiraEn ?? DateTime.Now,
                    TiempoRestante = CalcularTiempoRestante(reservaActiva.ExpiraEn ?? DateTime.Now)
                };
            }

            // Obtener historial de parqueos
            var historialParqueos = await _context.Parqueos
                .Include(p => p.Vehiculo)
                .Where(p => p.UsuarioId == user.Id)
                .OrderByDescending(p => p.FechaEntrada)
                .Take(10)
                .ToListAsync();

            model.HistorialParqueos = historialParqueos.Select(p => new HistorialParqueoDto
            {
                Id = p.Id,
                Placa = p.Vehiculo.Placa,
                TipoVehiculo = p.Vehiculo.Tipo,
                FechaEntrada = p.FechaEntrada,
                FechaSalida = p.FechaSalida,
                TiempoTotal = CalcularTiempoTotal(p.FechaEntrada, p.FechaSalida),
                TotalPagar = (decimal)p.TotalPagar,
                Estado = p.FechaSalida == null ? "Dentro" : "Finalizado"
            }).ToList();

            model.Vehiculos = await _context.Vehiculos
                .Where(v => v.UsuarioId == user.Id)
                .ToListAsync();

            return View(model);
        }

        private string CalcularTiempoRestante(DateTime expiraEn)
        {
            var tiempoRestante = expiraEn - DateTime.Now;
            if (tiempoRestante.TotalMinutes <= 0)
                return "Expirado";

            if (tiempoRestante.TotalHours >= 1)
                return $"{(int)tiempoRestante.TotalHours}h {tiempoRestante.Minutes}m";
            else
                return $"{(int)tiempoRestante.TotalMinutes}m";
        }

        private string CalcularTiempoTotal(DateTime entrada, DateTime? salida)
        {
            if (salida == null)
            {
                var tiempoActual = DateTime.Now - entrada;
                if (tiempoActual.TotalHours >= 1)
                    return $"{(int)tiempoActual.TotalHours}h {tiempoActual.Minutes}m";
                else
                    return $"{(int)tiempoActual.TotalMinutes}m";
            }

            var tiempoTotal = salida.Value - entrada;
            if (tiempoTotal.TotalHours >= 1)
                return $"{(int)tiempoTotal.TotalHours}h {tiempoTotal.Minutes}m";
            else
                return $"{(int)tiempoTotal.TotalMinutes}m";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservarCupo(int vehiculoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // O redirigir a Login
            }

            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == vehiculoId && v.UsuarioId == user.Id);

            if (vehiculo == null)
            {
                TempData["Error"] = "Vehículo no encontrado o no te pertenece.";
                return RedirectToAction("Dashboard");
            }

            // Verificar si ya tiene una reserva activa
            var reservaExistente = await _context.Reservas
                .AnyAsync(r => r.UsuarioId == user.Id && r.Activa && r.ExpiraEn > DateTime.Now);

            if (reservaExistente)
            {
                TempData["Error"] = "Ya tienes una reserva activa.";
                return RedirectToAction("Dashboard");
            }

            // Verificar cupos
            var cuposOcupados = await _context.Parqueos
                .CountAsync(p => p.Vehiculo.Tipo == vehiculo.Tipo && p.Estado == "Dentro");
            
            int cuposMaximos = vehiculo.Tipo == "Carro" ? 20 : 20;

            if (cuposOcupados < cuposMaximos)
            {
                TempData["Info"] = "Hay cupos disponibles, no necesitas reservar.";
                return RedirectToAction("Dashboard");
            }

            // Crear la reserva
            var reserva = new Reservas
            {
                VehiculoId = vehiculo.Id,
                UsuarioId = user.Id,
                FechaReserva = DateTime.Now,
                ExpiraEn = DateTime.Now.AddMinutes(30),
                Activa = true,
                Vehiculo = vehiculo,
                Usuario = user
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Reserva creada para el vehículo {vehiculo.Placa}. Tienes 30 minutos para ingresar.";
            return RedirectToAction("Dashboard");
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