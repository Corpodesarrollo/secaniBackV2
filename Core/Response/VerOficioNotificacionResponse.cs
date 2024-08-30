using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class VerOficioNotificacionResponse
    {
        public DateTime FechaNotificacion { get; set; }
        public string De { get; set; }
        public string? Para { get; set; }
        public string? ConCopia { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string Firma { get; set; }
        public string Adjuntos { get; set; }
    }
}
