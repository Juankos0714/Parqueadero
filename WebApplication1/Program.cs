using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using MySql.EntityFrameworkCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración de DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36)) // Cambia segn tu versión de MySQL
    ));

// Configuración de Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Esto debe ir ANTES de UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Necesario para las páginas de Identity

// Crear roles y tarifas al inicio de la aplicacin
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await MyApp.Data.SeedData.CreateRoles(serviceProvider);
    await MyApp.Data.SeedData.CreateTarifas(serviceProvider);
}

app.Run();