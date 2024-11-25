namespace Core.DTOs.MSTablasParametricas
{
    public class SubCategoriaAlertaDTO
    {
        public int Id { get; set; }
        public int CategoriaAlertaId { get; set; }
        public char Indicador { get; set; }
        public string SubCategoriaAlerta { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
    }
}
