namespace Core.DTOs
{
    public class NNAPendienteSivigilaDto
    {
        public long Id { get; set; }
        public long NoCaso { get; set; }
        public string? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? NombreNnaCompleto { get; set; }
        public string? DiagnosticoSiNo { get; set; }
        public long? IdContacto { get; set; }
        public string? NombreReportanteCompleto { get; set; }
        public string? Aseguradora { get; set; }
        public int? EAPBId { get; set; }
        public string? Municipio { get; set; }
        public string? MunicipioId { get; set; }
        public DateTime? FechaConsultaOrigenReporte { get; set; }
        public int? IdReporteSivigila { get; set; }
        public string? ArchivoDiagnostico { get; set; }
        public string? ArchivoParentesco { get; set; }
        // Datos NNA modal
        public DateTime? FechaNacimientoNNA { get; set; }
        public string? SexoNNA { get; set; }
        // Datos Reportante (cuidador) modal
        public string? NombreReportante { get; set; }
        public string? EmailReportante { get; set; }
        public string? CelularReportante { get; set; }
        public string? AliasReportante { get; set; }
        public string? TipoIdReportante { get; set; }
        public string? NumeroIdReportante { get; set; }
    }
}
