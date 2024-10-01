using Microsoft.AspNetCore.Identity;

namespace Core.Modelos.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? Alias { get; set; }
        public string? FullName { get; set; }
        public string Telefonos { get; set; }
        public int EstadoUsuarioId { get; set; }
        public string? EntidadId { get; set; }
        public string? Cargo { get; set; }
        public bool? Activo { get; set; }
        public string? ActivoName { get; set; }

    }
}
