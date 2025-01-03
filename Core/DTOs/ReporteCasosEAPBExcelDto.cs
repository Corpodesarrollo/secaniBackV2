namespace Core.DTOs
{
    public class ReporteCasosEAPBExcelDto
    {
        public DateTime? FechaNotificacion { get; set; }
        public string? NombreNNA { get; set; }
        public string? Edad { get; set; }
        public string? Sexo { get; set; }
        public int TiempoTranscurrido { get; set; }
        public string? Estado { get; set; }

        public string? CategoriaAlerta { get; set; }
        public DateTime? FechaEnvioRespuesta { get; set; }
        public string? DepartamentoProcedencia { get; set; }
        public string? DireccionProcedencia { get; set; }
        public string? Nacionalidad { get; set; }
        public string? SubcategoriaAlerta { get; set; }
        public string? Respuesta { get; set; }
        public string? MunicipioProcedencia { get; set; }
        public string? DepartamentoActual { get; set; }
        public string? Etnia { get; set; }
        public string? Observaciones { get; set; }
        public string? TipoIdentificacion { get; set; }
        public string? BarrioProcedencia { get; set; }
        public string? EstadoNNA { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? AreaProcedencia { get; set; }
        public string? RegimenAfiliacion { get; set; }
        public int? DiagnosticoNNA { get; set; }
        public int? IpsPrimaria { get; set; }
    }
}
