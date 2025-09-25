using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
namespace MyApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
        // Aquí agregas tus tablas (DbSet = tabla en la BD)
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Parqueos> Parqueos { get; set; }
        public DbSet<Reservas> Reservas { get; set; }
        public DbSet<Tarifas> Tarifas { get; set; }
        public DbSet<Vehiculos> Vehiculos { get; set; }
    }

}