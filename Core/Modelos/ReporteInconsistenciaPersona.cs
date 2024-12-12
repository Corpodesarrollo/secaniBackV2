namespace Core.Modelos
{
    public class ReporteInconsistenciaPersona
    {
        public long NNAId { get; set; }
        public DateTime FechaReporte { get; set; }
        public string TipoIdentificacionSIVIGILA { get; set; }
        public string TipoIdentificacion { get; set; }
        public string NumeroIdentificacionSIVIGILA { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string? PrimerNombre { get; set; }
        public string? PrimerNombreSIVIGILA { get; set; }
        public string? SegundoNombre { get; set; }
        public string? SegundoNombreSIVIGILA { get; set; }
        public string? PrimerApellido { get; set; }
        public string? PrimerApellidoSIVIGILA { get; set; }
        public string? SegundoApellido { get; set; }
        public string? SegundoApellidoSIVIGILA { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaNacimientoSIVIGILA { get; set; }
        public string? SexoId { get; set; }
        public string? SexoIdSIVIGILA { get; set; }
        public DateTime? FechaDefuncion { get; set; }
        public DateTime? FechaDefuncionSIVIGILA { get; set; }
    }
}
