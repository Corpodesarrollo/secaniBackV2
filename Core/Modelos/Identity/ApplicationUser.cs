using Microsoft.AspNetCore.Identity;

namespace Core.Modelos.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string Telefonos { get; set; }
        public int EstadoUsuarioId { get; set; }

    }
}
