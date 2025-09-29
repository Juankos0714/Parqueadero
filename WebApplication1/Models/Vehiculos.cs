using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Vehiculos
    {
        public int Id { get; set; }
        [StringLength(10)]
        public string Placa { get; set; }=string.Empty;
        [StringLength(50)]
        public string Tipo { get; set; }=string.Empty;
        public string UsuarioId { get; set; }
        public ApplicationUser Usuario { get; set; }
    }
}
