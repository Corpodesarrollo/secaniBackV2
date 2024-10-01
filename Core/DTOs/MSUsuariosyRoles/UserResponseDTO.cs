namespace Core.DTOs.MSUsuariosyRoles
{
    public class UserResponseDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
        public string? EntidadId { get; set; }
        public string? Cargo { get; set; }
        public string? Estado { get; set; }
    }
}

