using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public enum TipoVehiculo
    {
        Carro,
        Moto
    }

    public class Vehiculos
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "La placa debe tener entre 1 y 20 caracteres.")]
        public string Placa { get; set; }

        [Required(ErrorMessage = "El tipo de vehículo es obligatorio.")]
        [EnumDataType(typeof(TipoVehiculo))]
        public TipoVehiculo Tipo { get; set; }

        [ForeignKey("UsuarioId")]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }
    }
}
