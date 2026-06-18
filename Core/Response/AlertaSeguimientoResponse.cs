namespace Core.Response
{
    public class AlertaSeguimientoResponse
    {
        public long AlertaId { get; set; }
        public long SeguimientoId { get; set; }
        public long IdAlertaSeguimiento { get; set; }
        public string? Observaciones { get; set; }
        public int EstadoId { get; set; }
        public DateTime UltimaFechaSeguimiento { get; set; }
        public string NombreAlerta { get; set; }
        public string CategoriaAlerta { get; set; }
        public string SubcategoriaAlerta { get; set; }
        // BUG-LZ-055: trazabilidad por-alerta (antes solo a nivel seguimiento → duplicados/vacios)
        public string? EntidadAlerta { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? RespuestaEntidad { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        // Bug 2026-06-17: el modal "Respuesta de la entidad" reusaba alerta.NombreAlerta como
        // asunto (mostraba "7.A"). Ahora exponemos el asunto real de RespuestasAlerta y el
        // nombre del archivo adjunto si lo hay.
        public string? AsuntoRespuesta { get; set; }
        public string? ArchivoAdjuntoRespuesta { get; set; }
    }
}
