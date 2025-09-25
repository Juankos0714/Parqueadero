using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class Parqueos
    {
        public int Id { get; set; }   // Clave primaria con auto incremento

        // Relaciones
        public int VehiculoId { get; set; }
        public required Vehiculos Vehiculo { get; set; }

        public int UsuarioId { get; set; }
        public required Usuarios Usuario { get; set; }

        // Propiedades
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }  
        public required string Estado { get; set; }
        public double TotalPagar { get; set; }
    }
}
