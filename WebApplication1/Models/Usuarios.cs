using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    // Se extiende IdentityUser para aÃ±adir propiedades personalizadas
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        // El rol se manejarÃ¡ con el sistema de Roles de Identity, por lo que la propiedad Rol ya no es necesaria aquÃ­.
    }
}
