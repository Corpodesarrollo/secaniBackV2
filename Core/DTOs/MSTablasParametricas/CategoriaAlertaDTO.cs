namespace Core.DTOs.MSTablasParametricas
{
    public class CategoriaAlertaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public ICollection<SubCategoriaAlertaDTO>? SubCategorias { get; set; }
    }
}
