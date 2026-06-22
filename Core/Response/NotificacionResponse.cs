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
        // BUG-LZ 2026-06-20: adjunto que subio la EAPB al responder (vive en tabla
        // Adjuntos con Tipo=Respuesta y Referencia=respuestaAlerta.IdAlerta).
        public string? ArchivoAdjuntoRespuesta { get; set; }
        // BUG-LZ 2026-06-20: PDF autogenerado del oficio (membrete ICBF, datos NNA,
        // alerta, cierre, firma) que el agente envia al EAPB. Vive en tabla Adjuntos
        // con Tipo=Notificacion y Referencia=NotificacionEntidad.Id.
        public string? ArchivoOficio { get; set; }
    }
}
