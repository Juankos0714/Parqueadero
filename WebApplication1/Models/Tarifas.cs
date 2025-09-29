using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Tarifas
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string TipoVehiculo { get; set; }=string.Empty;
        [StringLength(50)]
        public string Ubicacion { get; set; } = string.Empty;   
        public double ValorHora { get; set; }   
    }
}
