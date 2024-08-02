namespace Core.Modelos.TablasParametricas
{
    public class TPSubCategoriaAlerta
    {
        public int Id { get; set; }
        public int CategoriaAlertaId { get; set; }
        public char Indicador { get; set; }
        public string SubCategoriaAlerta { get; set; }
    }
}
