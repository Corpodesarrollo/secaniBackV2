namespace Core.Modelos
{
    public class TPModuloComponenteObjeto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? ModuloComponenteObjetoIdPadre { get; set; }
        public int Orden { get; set; }
        public int Activo { get; set; }
    }
}
