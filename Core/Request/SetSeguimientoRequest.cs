using Core.DTOs;

namespace Core.Request
{
    public class SetSeguimientoRequest
    {
        public long NNAId { get; set; }

        DateTime fechaSeguimiento = DateTime.Now;
        public DateTime FechaSeguimiento
        {
            get
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaColombia = TimeZoneInfo.ConvertTimeFromUtc(fechaSeguimiento.ToUniversalTime(), timeZone);
                return fechaColombia;
            }
            set
            {
                fechaSeguimiento = value;
            }
        }
        public int EstadoId { get; set; }
        public long ContactoNNAId { get; set; }
        public string? Telefono { get; set; }
        public string? UsuarioId { get; set; }
        public long SolicitanteId { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public bool? TieneDiagnosticos { get; set; }
        public string? ObservacionesSolicitante { get; set; }
        public string? ObservacionAgente { get; set; }
        public string? UltimaActuacionAsunto { get; set; }
        public DateTime? UltimaActuacionFecha { get; set; }
        public string? NombreRechazo { get; set; }
        public string? ParentescoRechazo { get; set; }
        public string? RazonesRechazo { get; set; }
        public int[]? Alertas { get; set; }
        public AlertaSeguimientoDto[]? alertasPendientes { get; set; }
    }
}
