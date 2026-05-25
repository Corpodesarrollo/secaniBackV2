namespace Core.DTOs.MSPermisos
{
    public class ModuloRequestDTO
    {
        public string Nombre { get; set; }
        public String Path { get; set; }
        public String Icon { get; set; }
        public int ModuloComponenteObjetoIdPadre { get; set; }
        public int Orden { get; set; }
        public int Activo { get; set; }
    }
}
