using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace Core.DTOs.Reportes
{
    public class ReporteDetalleRegDepuradosDTO
    {
        //NNA
        public long Id { get; set; }
        public int IdReporteDepuracion { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public int? OrigenReporteId { get; set; }
        public string OrigenReporte { get; set; } //calculado
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public int? DiagnosticoId { get; set; }
        public string Diagnostico { get; set; } //calculado
        public DateTime? FechaNacimiento { get; set; }
        public int Edad { get; set; } //calculado
        public string? SexoId { get; set; }
        public string Sexo { get; set; } //calculado
        public string? TipoIdentificacionId { get; set; }
        public string? TipoIdentificacion { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? PaisId { get; set; }
        public string? Pais { get; set; }
        public string? EtniaId { get; set; }
        public string? Etnia { get; set; }
        public string? MunicipioNacimientoId { get; set; }
        public string DepartamentoNacimiento { get; set; } //calculado
        public string MunicipioNacimiento { get; set; } //calculado
        public string? GrupoPoblacionId { get; set; }
        public string? GrupoPoblacion { get; set; }
        public string? ResidenciaOrigenMunicipioId { get; set; }
        public string ResidenciaOrigenDepartamento { get; set; } //calculado
        public string ResidenciaOrigenMunicipio { get; set; } //calculado
        public string? ResidenciaOrigenBarrio { get; set; }
        public string? ResidenciaOrigenAreaId { get; set; }
        public string AreaProcedencia { get; set; } //calculado
        public string? ResidenciaOrigenDireccion { get; set; }
        public string? ResidenciaOrigenEstratoId { get; set; }
        public string? ResidenciaActualTelefono { get; set; }
        public string? DepartamentoTratamientoId { get; set; }
        public string DepartamentoTratamiento { get; set; } //calculado
        public int? EstadoIngresoEstrategiaId { get; set; }
        public string EstadoIngresoEstrategia { get; set; } //calculado
        public DateTime? FechaIngresoEstrategia { get; set; }
        public string? TipoRegimenSSId { get; set; }
        public string? TipoRegimenSS { get; set; }
        public int? EPSId { get; set; }
        public string EPS { get; set; } //calculado
        public int? IPSId { get; set; }
        public string IPS { get; set; } //calculado
        public string? CuidadorNombres { get; set; }
        public string? CuidadorParentescoId { get; set; }
        public string? CuidadorParentesco { get; set; }
        public string? CuidadorEmail { get; set; }
        public string? CuidadorTelefono { get; set; }
        public string Agente { get; set; } //calculado

    }
}
