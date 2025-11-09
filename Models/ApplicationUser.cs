using Microsoft.AspNetCore.Identity;
namespace TiendaGamer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? NombreCompleto { get; set; }
    }
}
