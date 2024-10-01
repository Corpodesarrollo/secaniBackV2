namespace Core.DTOs.MSPermisos
{
    public class ModuloResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public String Path { get; set; } = String.Empty;
        public String Icon { get; set; } = String.Empty;
        public int? ModuloComponenteObjetoIdPadre { get; set; }
        public int Orden { get; set; }
        public int Activo { get; set; }
    }
}
