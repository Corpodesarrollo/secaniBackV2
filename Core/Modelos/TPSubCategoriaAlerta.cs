namespace Core.Modelos
{
    public class TPSubCategoriaAlerta
    {
        public int Id { get; set; }
        public string SubCategoriaAlerta { get; set; }
        public int CategoriaAlertaId { get; set; }
        public string Indicador { get; set; }
    }
}
