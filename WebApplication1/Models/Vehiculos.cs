namespace WebApplication1.Models
{
    public class Vehiculos
    {
        public int Id { get; set; }
        public string Placa { get; set; }=string.Empty;
        public string Tipo { get; set; }=string.Empty;
        public int UsuarioId { get; set; }
        public required Usuarios Usuario { get; set; }
    }
}
