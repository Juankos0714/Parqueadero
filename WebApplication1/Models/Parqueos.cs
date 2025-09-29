using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Parqueos
    {
        public int Id { get; set; }   // Clave primaria con auto incremento

        // Relaciones
        public int VehiculoId { get; set; }
        public required Vehiculos Vehiculo { get; set; }

        public string UsuarioId { get; set; }
        public required ApplicationUser Usuario { get; set; }

        // Propiedades
        public DateTime FechaEntrada { get; set; }
        public DateTime? FechaSalida { get; set; }
        [StringLength(50)]
        public required string Estado { get; set; }
        public double TotalPagar { get; set; }
    }
}
