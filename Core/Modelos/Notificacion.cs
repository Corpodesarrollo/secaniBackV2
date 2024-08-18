using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Notificacion : BaseEntity
    {
        public long AlertaSeguimientoId { get; set; }
        public DateTime FechaNotificacion { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string RespuestaEntidad { get; set; }
        public string EntidadId { get; set; }
    }
}
