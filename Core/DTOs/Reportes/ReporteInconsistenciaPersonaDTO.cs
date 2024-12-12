namespace Core.DTOs.Reportes
{
    public class ReporteInconsistenciaPersonaDTO
    {
        public long NNAId { get; set; }
        public DateTime FechaReporte { get; set; }
        public string TipoIdentificacionSIVIGILA { get; set; }
        public string TipoIdentificacion { get; set; }
        public bool TipoIdentificacionInconsistente { get; set; } = false;
        public string NumeroIdentificacionSIVIGILA { get; set; }
        public string NumeroIdentificacion { get; set; }
        public bool NumeroIdentificacionInconsistente { get; set; } = false;
        public string? PrimerNombre { get; set; }
        public string? PrimerNombreSIVIGILA { get; set; }
        public bool PrimerNombreInconsistente { get; set; } = false;
        public string? SegundoNombre { get; set; }
        public string? SegundoNombreSIVIGILA { get; set; }
        public bool SegundoNombreInconsistente { get; set; } = false;
        public string? PrimerApellido { get; set; }
        public string? PrimerApellidoSIVIGILA { get; set; }
        public bool PrimerApellidoInconsistente { get; set; } = false;
        public string? SegundoApellido { get; set; }
        public string? SegundoApellidoSIVIGILA { get; set; }
        public bool SegundoApellidoInconsistente { get; set; } = false;
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaNacimientoSIVIGILA { get; set; }
        public bool FechaNacimientoInconsistente { get; set; } = false;
        public string? SexoId { get; set; }
        public string? SexoIdSIVIGILA { get; set; }
        public bool SexoIdInconsistente { get; set; } = false;
        public DateTime? FechaDefuncion { get; set; }
        public DateTime? FechaDefuncionSIVIGILA { get; set; }
        public bool FechaDefuncionInconsistente { get; set; } = false;
    }
}
