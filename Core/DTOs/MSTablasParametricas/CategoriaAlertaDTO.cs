namespace Core.DTOs.MSTablasParametricas
{
    public class CategoriaAlertaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public ICollection<SubCategoriaAlertaDTO>? SubCategorias { get; set; }
    }
}
