namespace Core.DTOs.Reportes
{
    public class ReporteDinamicoSeguimientoDTO
    {
        //NNA
        public long SeguimientoId { get; set; }
        public long NNAId { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public int? DiagnosticoId { get; set; }
        public string Diagnostico { get; set; } //calculado
        public string? TipoIdentificacionId { get; set; }
        public string? TipoIdentificacion { get; set; } //calculado
        public string? NumeroIdentificacion { get; set; }
        public string? TipoRegimenSSId { get; set; }
        public string? TipoRegimenSS { get; set; } //calculado
        public int? EAPBId { get; set; }
        public string EAPB { get; set; } //calculado
        public int EstadoId { get; set; }
        public string Estado { get; set; } //calculado
        public DateTime? FechaConsultaDiagnostico { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
        public int? MotivoNoDiagnosticoId { get; set; }
        public string? MotivoNoDiagnostico { get; set; } //calculado
        public string? MotivoNoDiagnosticoOtro { get; set; }
        public DateTime? FechaInicioTratamiento { get; set; }
        public int? IPSId { get; set; }
        public string IPS { get; set; } //calculado
        public bool? Recaida { get; set; }
        public int? CantidadRecaidas { get; set; }
        public DateTime? FechaUltimaRecaida { get; set; }
        public bool? TrasladosHaSidoTrasladadodeInstitucion { get; set; }
        public string? ResidenciaActualMunicipioId { get; set; }
        public string ResidenciaActualMunicipio { get; set; } //calculado
        public string ResidenciaActualDepartamento { get; set; } //calculado
        public string? ResidenciaActualBarrio { get; set; }
        public string? ResidenciaActualAreaId { get; set; }
        public string ResidenciaActualArea { get; set; } //calculado
        public string? ResidenciaActualDireccion { get; set; }
        public string? ResidenciaActualEstratoId { get; set; }
        public bool? TrasladoTieneCapacidadEconomica { get; set; }
        public bool? TrasladoEAPBSuministroApoyo { get; set; }
        public bool? TrasladosServiciosdeApoyoOportunos { get; set; }
        public string? CuidadorNombres { get; set; }
        public int? CuidadorParentescoId { get; set; }
        public string CuidadorParentesco { get; set; } //calculado
        public string? CuidadorEmail { get; set; }
        public string? CuidadorTelefono { get; set; }

        //Seguimiento
        public DateTime? FechaSeguimiento { get; set; } //Seguimiento
        public string? ObservacionesSolicitante { get; set; } //Seguimiento
        public string? ObservacionAgente { get; set; }  //Seguimiento
    }
}
