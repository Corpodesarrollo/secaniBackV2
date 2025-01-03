namespace Core.Modelos
{
    public enum TipoRegistro
    {
        Nuevo = 1,
        Duplicado = 2,
        SegundaNeoplastia = 3,
        Recaida = 4
    }

    public class ReporteDepuracionDetalle
    {
        public int Id { get; set; }
        public int IdReporteDepuracion { get; set; }
        public long IdNNA { get; set; }
        public int TipoRegistro { get; set; }

        // Propiedad de solo lectura para devolver el texto correspondiente al valor de TipoRegistro
        public string TipoRegistroString
        {
            get
            {
                return TipoRegistro switch
                {
                    (int)Modelos.TipoRegistro.Nuevo => "Nuevo",
                    (int)Modelos.TipoRegistro.Duplicado => "Duplicado",
                    (int)Modelos.TipoRegistro.SegundaNeoplastia => "Segunda Neoplastia",
                    (int)Modelos.TipoRegistro.Recaida => "Recaída",
                    _ => "Desconocido" // Valor por defecto para casos no contemplados
                };
            }
        }
    }
}
