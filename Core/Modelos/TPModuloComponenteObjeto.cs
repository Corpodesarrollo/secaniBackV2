namespace Core.Modelos
{
    public class TPModuloComponenteObjeto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public int? ModuloComponenteObjetoIdPadre { get; set; }
        public int Orden { get; set; }
        public int Activo { get; set; }
    }
}
