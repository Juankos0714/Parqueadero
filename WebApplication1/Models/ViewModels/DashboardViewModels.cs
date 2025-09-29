using System.Collections.Generic;

namespace WebApplication1.Models.ViewModels
{
    public class DashboardAprendizViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public int CantidadVehiculos { get; set; }
        public ReservaActivaDto? ReservaActiva { get; set; }
        public List<HistorialParqueoDto> HistorialParqueos { get; set; } = new();
        public List<Vehiculos> Vehiculos { get; set; } = new();
    }
        
    public class DashboardFuncionarioViewModel
    {
        public int CarrosDentro { get; set; }
        public int MotasDentro { get; set; }
        public decimal IngresosHoy { get; set; }
        public int ReservasActivas { get; set; }
        public List<VehiculoActivoDto> VehiculosActivos { get; set; } = new();
        public List<VehiculoDentroDto> VehiculosDentro { get; set; } = new();
    }

    public class ReservaActivaDto
    {
        public int Id { get; set; }
        public string TiempoRestante { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public string PlacaVehiculo { get; set; } = string.Empty;
    }

    public class HistorialParqueoDto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        public string TiempoTotal { get; set; } = string.Empty;
        public decimal TotalPagar { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class VehiculoActivoDto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string NombrePropietario { get; set; } = string.Empty;
        public DateTime HoraEntrada { get; set; }
        public string TiempoTranscurrido { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class VehiculoDentroDto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string NombrePropietario { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaEntrada { get; set; }
    }

    public class ReportesViewModel
    {
        public decimal IngresosTotales { get; set; }
        public int VehiculosAtendidos { get; set; }
        public int CarrosAtendidos { get; set; }
        public int MotosAtendidas { get; set; }
        public string TiempoPromedio { get; set; } = string.Empty;
        public int OcupacionPromedio { get; set; }
        public List<HistorialDetalladoDto> HistorialDetallado { get; set; } = new();
        public List<IngresoDiarioDto> IngresosDiarios { get; set; } = new();
        public List<PagoPorVehiculoDto> PagosPorVehiculo { get; set; } = new();
        public int CarrosDentro { get; set; }
        public int MotosDentro { get; set; }
        public int CarrosFuera { get; set; }
        public int MotosFuera { get; set; }
    }

    public class PagoPorVehiculoDto
    {
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public string NombrePropietario { get; set; } = string.Empty;
        public decimal TotalPagado { get; set; }
    }

    public class HistorialDetalladoDto
    {
        public DateTime Fecha { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public string NombrePropietario { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;
        public string HoraEntrada { get; set; } = string.Empty;
        public string HoraSalida { get; set; } = string.Empty;
        public string TiempoTotal { get; set; } = string.Empty;
        public decimal TarifaHora { get; set; }
        public decimal TotalPagado { get; set; }
    }

    public class IngresoDiarioDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalIngresos { get; set; }
        public int CantidadVehiculos { get; set; }
    }
}