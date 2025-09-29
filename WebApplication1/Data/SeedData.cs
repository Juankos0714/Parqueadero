using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using MyApp.Data;

namespace MyApp.Data
{
    public static class SeedData
    {
        public static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Aprendiz", "Funcionario" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static async Task CreateTarifas(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await context.Tarifas.AnyAsync())
            {
                var tarifas = new List<Tarifas>
                {
                    new Tarifas { TipoVehiculo = "Carro", Ubicacion = "Dentro", ValorHora = 2000 },
                    new Tarifas { TipoVehiculo = "Carro", Ubicacion = "Fuera", ValorHora = 1500 },
                    new Tarifas { TipoVehiculo = "Moto", Ubicacion = "Dentro", ValorHora = 1500 },
                    new Tarifas { TipoVehiculo = "Moto", Ubicacion = "Fuera", ValorHora = 1000 }
                };

                await context.Tarifas.AddRangeAsync(tarifas);
                await context.SaveChangesAsync();
            }
        }
    }
}
