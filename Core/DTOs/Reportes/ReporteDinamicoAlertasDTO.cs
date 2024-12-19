namespace Core.DTOs.Reportes
{
    public class ReporteDinamicoAlertasDTO
    {
        public DateTime FechaNotificacion { get; set; } //UltimaFechaSeguimiento AlertaSeguimiento
        public DateTime? FechaResolucion { get; set; } //UltimaFechaSeguimiento AlertaSeguimiento EstadoId a resuelto
        public int Notificaciones { get; set; } = 0; //Cantidad de Notificaciones con AlertaSeguimientoId

        //NNA
        public long NNAId { get; set; } //AlertaSeguimiento => SeguimientoId => NNAId
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public int Edad { get; set; } //calculado
        public int? DiagnosticoId { get; set; }
        public string? Diagnostico { get; set; } //calculado
        public string? ResidenciaActualTelefono { get; set; }
        public string? ResidenciaActualMunicipioId { get; set; }
        public string? ResidenciaActualMunicipio { get; set; } //calculado
        public string? ResidenciaActualDepartamento { get; set; } //calculado
        public string? ResidenciaActualDireccion { get; set; }
        public string? TrasladosQuienAsumioCostosTraslado { get; set; }
        public string? TrasladosQuienAsumioCostosVivienda { get; set; }
        public bool? TratamientoHaDejadodeAsistir { get; set; }
        public string? TipoSeguimiento { get; set; } //GetLastSeguimiento
        public string? Agente { get; set; } //GetLastSeguimiento
        public int? EPSId { get; set; }
        public string? EPS { get; set; } //calculado
        public string? CuidadorEmail { get; set; }
        public int? TratamientoCuantoTiemposinAsistir { get; set; }
        public string? TratamientoUnidadMedidaIdTiempoId { get; set; }
        public string? TratamientoUnidadMedidaTiempo { get; set; } //calculado
        public string? TratamientoCausasInasistenciaId { get; set; }
        public string? TratamientoCausasInasistencia { get; set; } //calculado
        public string? TratamientoCausasInasistenciaOtra { get; set; }
        public string? CategoriaAlerta { get; set; } = string.Empty; //alertaseguimiento => Alerta => Descripcion 
        public string? SubCategoriaAlerta { get; set; } = string.Empty; //alertaseguimiento => Alerta => SubCategoriaAlertaId => TPSubCategoriaAlerta => SubCategoriaalerta
        public string? EstadoAlerta { get; set; } = string.Empty; //alertaseguimiento => EstadoId => TPEstadoAlerta
        public bool? TratamientoEstudiaActualmente { get; set; }
        public bool? TratamientoHaDejadodeAsistirColegio { get; set; }
        public int? TratamientoTiempoInasistenciaColegio { get; set; }
        public string? TratamientoTiempoInasistenciaUnidadMedidaId { get; set; }
        public string? TratamientoTiempoInasistenciaUnidadMedida { get; set; } //calculado
        public DateTime? FechaRespuesta { get; set; } //Notificacion alertaseguimientoid
        public string? RespuestaEntidad { get; set; } //Notificacion alertaseguimientoid
        public bool? TratamientoHaSidoInformadoClaramente { get; set; }
        public bool? TrasladosHaSidoTrasladadodeInstitucion { get; set; }
        public bool? TrasladosHaSolicitadoApoyoFundacion { get; set; }
        public string? TrasladosNombreFundacion { get; set; }
        public string? TrasladosApoyoRecibidoxFundacion { get; set; }
    }
}
