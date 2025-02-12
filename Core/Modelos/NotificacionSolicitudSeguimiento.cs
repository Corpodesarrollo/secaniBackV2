using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class NotificacionSolicitudSeguimiento 
    {
        [Key]
        public string? Id { get; set; }
        public string? CuidadorId { get; set; }
        public long? NNAId { get; set; }
        public string? AgenteSeguimientoId { get; set; }
        public int? TotalEnvios { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? DateCreated { get; set; }

    }
}
