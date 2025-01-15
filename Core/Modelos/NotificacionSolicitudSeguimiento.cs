using Core.Modelos.Common;

namespace Core.Modelos
{
    public class NotificacionSolicitudSeguimiento 
    {
        public string? CuidadorId { get; set; }
        public long? NNAId { get; set; }
        public string? AgenteSeguimientoId { get; set; }
        public byte? TotalEnvios { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? DateCreated { get; set; }

    }
}
