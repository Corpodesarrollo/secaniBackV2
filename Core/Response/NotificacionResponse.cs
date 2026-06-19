using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class NotificacionResponse
    {
        public string? EntidadNotificada {  get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? AsuntoNotificacion { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? Respuesta { get; set; }
        public string? Notificacion { get; set; }
        // BUG-LZ 2026-06-19: campos para alimentar el modal "Ver notificacion"
        // (carousel) en /consultar-alertas con data real en lugar del mock.
        public string? EmailDe { get; set; }
        public string? EmailPara { get; set; }
        public string? EmailConCopia { get; set; }
        public string? Firma { get; set; }
        public string? ArchivoAdjunto { get; set; }
    }
}
