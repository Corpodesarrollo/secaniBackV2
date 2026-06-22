namespace Core.DTOs
{
    public class FiltroNNADto
    {
        public long? NoCaso { get; set; }
        public long? IdNNA { get; set; }
        public string? NombreNNA { get; set; }
        public string? NoDocumento { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public string? AgenteAsignado { get; set; }
        public int? EstadoId { get; set; }
        public string? Estado { get; set; }
        public string? EstadoDescripcion { get; set; }
        public string? EstadoColorBG { get; set; }
        public string? EstadoColorText { get; set; }
        // BUG-LZ 2026-06-20: id del ultimo seguimiento del NNA para enlazar
        // /gestion/consultar-alertas/{idSeguimiento} desde Historico NNA.
        public long? IdSeguimiento { get; set; }
    }
}
