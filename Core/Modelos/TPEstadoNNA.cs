namespace Core.Modelos
{
    public class TPEstadoNNA
    {
        public int? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? ColorBG { get; set; }
        public string? ColorText { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
