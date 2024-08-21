namespace Core.Request
{
    public class DiagnosticoTratamientoRequest
    {
        public int IdSeguimiento { get; set; }
        public string IdDiagnostico { get; set; }
        public DateTime? FechaConsulta { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
        public DateTime? FechaInicioTratamiento { get; set; }
        public int? IdIPS { get; set; }
        public bool Recaidas { get; set; }
        public int? NumeroRecaidas { get; set; }
        public DateTime? FechaUltimaRecaida { get; set; }
        public string? IdMotivoNoDiagnostico { get; set; }
        public string? RazonNoDiagnostico { get; set; }
    }
}
