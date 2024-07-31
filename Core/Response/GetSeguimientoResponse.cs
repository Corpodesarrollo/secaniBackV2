namespace Core.response
{
    public class GetSeguimientoResponse
    {

        public long Id { get; set; }
        public long NNAId { get; set; }
        public DateTime FechaSeguimiento { get; set; }
        public int EstadoId { get; set; }
        public long ContactoNNAId { get; set; }
        public string? Telefono { get; set; }
       
        public long SolicitanteId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool TieneDiagnosticos { get; set; }
        public string? ObservacionesSolicitante { get; set; }

        public string? UsuarioId { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime FechaNotificacionSIVIGILA { get; set; }
        public int CantidadAlertas  { get; set; }
    }
}
