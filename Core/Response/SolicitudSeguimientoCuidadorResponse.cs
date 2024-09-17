namespace Core.Response
{
    public class SolicitudSeguimientoCuidadorResponse
    {
        public long NoCaso { get; set; }
        public string NombreCompletoNNA { get; set; }
        public string SexoNNa { get; set; }
        public DateTime? FechaNacimientoNNA { get; set; }
        public string NombreSolicitante { get; set; }
        public string CorreoSolicitante { get; set; }
        public string TelefonoSolicitante { get; set; }
        public string DiagnosticoNNA { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
        public string ObservacionSolicitante { get; set; }
        public string NombreAdjunto { get; set; }
        public string Base64Adjunto { get; set; }
        public int? DiagnosticoId { get; set; }
    }
}
