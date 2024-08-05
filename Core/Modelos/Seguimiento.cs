using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Seguimiento : BaseEntity
    {
        public long NNAId { get; set; }
        public DateTime FechaSeguimiento { get; set; }
        public int EstadoId { get; set; }
        public long ContactoNNAId { get; set; }
        public string Telefono { get; set; }
        public string UsuarioId { get; set; }
        public long SolicitanteId { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool TieneDiagnosticos { get; set; }
        public string ObservacionesSolicitante { get; set; }
        public string ObservacionAgente { get; set; }
        public string? UltimaActuacionAsunto { get; set; }
        public DateTime? UltimaActuacionFecha { get; set; }
    }
}
