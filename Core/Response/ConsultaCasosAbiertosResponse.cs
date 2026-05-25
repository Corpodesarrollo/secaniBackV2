namespace Core.Response
{
    public class ConsultaCasosAbiertosResponse
    {
        public int NoCaso { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? Nombre { get; set; }
        public int Estado { get; set; }
        public string? AsuntoUltimaActuacion { get; set; }
        public DateTime? FechaUltimaActuacion { get; set; }
        public AlertaSeguimientoResponse[]? Alertas { get; set; }
        public long SeguimientoId { get; set; }
    }
}
