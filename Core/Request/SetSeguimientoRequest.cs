namespace Core.Request
{
    public class SetSeguimientoRequest
    {
        public long NNAId { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public int EstadoId { get; set; }
        public long ContactoNNAId { get; set; }
        public string? Telefono { get; set; }
        public string? UsuarioId { get; set; }
        public long SolicitanteId { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public bool? TieneDiagnosticos { get; set; }
        public string? ObservacionesSolicitante { get; set; }
        public string? ObservacionAgente { get; set; }
        public string? UltimaActuacionAsunto { get; set; }
        public DateTime? UltimaActuacionFecha { get; set; }
        public string? NombreRechazo { get; set; }
        public string? ParentescoRechazo { get; set; }
        public string? RazonesRechazo { get; set; }
        public int[]? Alertas { get; set; }
    }
}
