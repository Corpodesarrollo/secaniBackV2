namespace Core.Modelos
{
    public class TPSubCategoriaAlerta
    {
        public int Id { get; set; }
        public string SubCategoriaAlerta { get; set; }
        public int CategoriaAlertaId { get; set; }
        public string Indicador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
