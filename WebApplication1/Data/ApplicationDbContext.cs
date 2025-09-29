using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace MyApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // Se cambia a IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        // El DbSet<ApplicationUser> (Usuarios) es manejado por IdentityDbContext
        public DbSet<Parqueos> Parqueos { get; set; }
        public DbSet<Reservas> Reservas { get; set; }
        public DbSet<Tarifas> Tarifas { get; set; }
        public DbSet<Vehiculos> Vehiculos { get; set; }
    }

}