namespace Core.Modelos.Common
{
    public abstract class TPBase
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
    }
}
