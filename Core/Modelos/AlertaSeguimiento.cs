using Core.Modelos.Common;

namespace Core.Modelos
{
    public class AlertaSeguimiento : BaseEntity
    {
        public long AlertaId { get; set; }
        public long SeguimientoId { get; set; }
        public string? Observaciones { get; set; }
        public int EstadoId { get; set; }
        public DateTime UltimaFechaSeguimiento { get; set; }
    }
}
