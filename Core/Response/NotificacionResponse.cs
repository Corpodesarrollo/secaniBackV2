using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class NotificacionResponse
    {
        public string EntidadNotificada {  get; set; }
        public DateTime FechaNotificacion { get; set; }
        public string AsuntoNotificacion { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string Respuesta { get; set; }
        public string Notificacion { get; set; }
    }
}
