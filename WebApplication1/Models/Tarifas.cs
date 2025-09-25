namespace WebApplication1.Models
{
    public class Tarifas
    {
        public int Id { get; set; }
        public string TipoVehiculo { get; set; }=string.Empty;
        public string Ubicacion { get; set; } = string.Empty;   
        public double ValorHora { get; set; }   
    }
}
