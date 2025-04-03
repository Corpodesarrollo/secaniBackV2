namespace Core.DTOs.MSTablasParametricas
{
    public class GenericTPDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
}
