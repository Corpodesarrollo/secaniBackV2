namespace Core.DTOs.Reportes
{
    public class ReporteDinamicoSeguimientoDTO
    {
        //NNA
        public long SeguimientoId { get; set; }
        public long Id { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public int? DiagnosticoId { get; set; }
        public string Diagnostico { get; set; } //calculado
        public string? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? TipoRegimenSSId { get; set; }
        public int? EAPBId { get; set; }
        public string EAPB { get; set; }
    }
}
