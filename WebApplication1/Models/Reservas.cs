namespace WebApplication1.Models
{
    public class Reservas
    {
        public int Id { get; set; }
        public int VehiculoId { get; set; }
        public required Vehiculos Vehiculo { get; set; }

        public int UsuarioId { get; set; }
        public required Usuarios Usuario { get; set; }
        public DateTime FechaReserva {  get; set; }
        public DateTime? ExpiraEn {  get; set; }
        public bool Activa { get; set; }

    }
}
