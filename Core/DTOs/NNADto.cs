﻿namespace Core.DTOs
{
    public class NNADto
    {
        public long Id { get; set; }
        public int? estadoId { get; set; }
        public string? ResidenciaActualCategoriaId { get; set; }
        public string? ResidenciaActualMunicipioId { get; set; }
        public string? ResidenciaActualBarrio { get; set; }
        public string? ResidenciaActualAreaId { get; set; }
        public string? ResidenciaActualDireccion { get; set; }
        public string? ResidenciaActualEstratoId { get; set; }
        public string? ResidenciaActualTelefono { get; set; }
        public string? ResidenciaOrigenCategoriaId { get; set; }
        public string? ResidenciaOrigenMunicipioId { get; set; }
        public string? ResidenciaOrigenBarrio { get; set; }
        public string? ResidenciaOrigenAreaId { get; set; }
        public string? ResidenciaOrigenDireccion { get; set; }
        public string? ResidenciaOrigenEstratoId { get; set; }
        public string? ResidenciaOrigenTelefono { get; set; }
        public DateTime? FechaNotificacionSIVIGILA { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? MunicipioNacimientoId { get; set; }
        public string? SexoId { get; set; }
        public string? TipoRegimenSSId { get; set; }
        public int? EAPBId { get; set; }
        public string EAPBNombre {  get; set; }
        public int? EPSId { get; set; }
        public string? EPSNombre { get; set; }
        public int? IPSId { get; set; }
        public string? IPSNombre { get; set; }
        public string? GrupoPoblacionId { get; set; }
        public string? EtniaId { get; set; }
        public int? EstadoIngresoEstrategiaId { get; set; }
        public DateTime? FechaIngresoEstrategia { get; set; }
        public int? OrigenReporteId { get; set; }
        public DateTime? FechaConsultaOrigenReporte { get; set; }
        public string? TipoCancerId { get; set; }
        public DateTime? FechaInicioSintomas { get; set; }
        public DateTime? FechaHospitalizacion { get; set; }
        public DateTime? FechaDefuncion { get; set; }
        public string? MotivoDefuncion { get; set; }
        public DateTime? FechaInicioTratamiento { get; set; }
        public bool? Recaida { get; set; }
        public int? CantidadRecaidas { get; set; }
        public DateTime? FechaUltimaRecaida { get; set; }
        public string? TipoDiagnosticoId { get; set; }
        public int? DiagnosticoId { get; set; }
        public DateTime? FechaDiagnostico { get; set; }
        public string? MotivoNoDiagnosticoId { get; set; }
        public string? MotivoNoDiagnosticoOtro { get; set; }
        public DateTime? FechaConsultaDiagnostico { get; set; }
        public string? DepartamentoTratamientoId { get; set; }
        public bool? IPSIdTratamiento { get; set; }
        public bool? PropietarioResidenciaActual { get; set; }
        public string? PropietarioResidenciaActualOtro { get; set; }
        public bool? TrasladoTieneCapacidadEconomica { get; set; }
        public bool? TrasladoEAPBSuministroApoyo { get; set; }
        public bool? TrasladosServiciosdeApoyoOportunos { get; set; }
        public bool? TrasladosServiciosdeApoyoCobertura { get; set; }
        public bool? TrasladosHaSolicitadoApoyoFundacion { get; set; }
        public string? TrasladosNombreFundacion { get; set; }
        public string? TrasladosApoyoRecibidoxFundacion { get; set; }
        public bool? DifAutorizaciondeMedicamentos { get; set; }
        public bool? DifEntregaMedicamentosLAP { get; set; }
        public bool? DifEntregaMedicamentosNoLAP { get; set; }
        public bool? DifAsignaciondeCitas { get; set; }
        public bool? DifHanCobradoCuotasoCopagos { get; set; }
        public bool? DifAutorizacionProcedimientos { get; set; }
        public bool? DifRemisionInstitucionesEspecializadas { get; set; }
        public bool? DifMalaAtencionIPS { get; set; }
        public int? DifMalaAtencionNombreIPSId { get; set; }
        public bool? DifFallasenMIPRES { get; set; }
        public bool? DifFallaConvenioEAPBeIPSTratante { get; set; }
        public int? CategoriaAlertaId { get; set; }
        public int? SubcategoriaAlertaId { get; set; }
        public bool? TrasladosHaSidoTrasladadodeInstitucion { get; set; }
        public int? TrasladosNumerodeTraslados { get; set; }
        public int? TrasladosIPSId { get; set; }
        public bool? TrasladosHaRecurridoAccionLegal { get; set; }
        public string? TrasladosTipoAccionLegalId { get; set; }
        public bool? TratamientoHaDejadodeAsistir { get; set; }
        public int? TratamientoCuantoTiemposinAsistir { get; set; }
        public string? TratamientoUnidadMedidaIdTiempoId { get; set; }
        public string? TratamientoCausasInasistenciaId { get; set; }
        public string? TratamientoCausasInasistenciaOtra { get; set; }
        public bool? TratamientoEstudiaActualmente { get; set; }
        public bool? TratamientoHaDejadodeAsistirColegio { get; set; }
        public int? TratamientoTiempoInasistenciaColegio { get; set; }
        public string? TratamientoTiempoInasistenciaUnidadMedidaId { get; set; }
        public bool? TratamientoHaSidoInformadoClaramente { get; set; }
        public string? TratamientoObservaciones { get; set; }
        public string? CuidadorNombres { get; set; }
        public string? CuidadorParentescoId { get; set; }
        public string? CuidadorEmail { get; set; }
        public string? CuidadorTelefono { get; set; }
        public string? SeguimientoLoDesea { get; set; }
        public string? SeguimientoMotivoNoLoDesea { get; set; }
        public DateTime DateDeleted { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? DeletedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public string? UpdatedByUserId { get; set; }
        public string? OrigenReporteOtro { get; set; }
        public string? PaisId { get; set; }
        public string? TrasladosMotivoAccionLegal { get; set; }
        public string? TrasladosPropietarioResidenciaActualId { get; set; }
        public string? TrasladosPropietarioResidenciaActualOtro { get; set; }
        public string? TrasladosQuienAsumioCostosTraslado { get; set; }
        public string? TrasladosQuienAsumioCostosVivienda { get; set; }
        public bool? TratamientoRequirioCambiodeCiudad { get; set; }
    }
}
