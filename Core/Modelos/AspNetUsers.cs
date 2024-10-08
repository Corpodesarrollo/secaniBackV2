using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos
{
    [Table("AspNetUsers")]
    public class AspNetUsers
    {
        public string Id { get; set; }

        public string? Alias { get; set; }
        public string? FullName { get; set; }
        public string Telefonos { get; set; }
        public int EstadoUsuarioId { get; set; }
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

    }
}
