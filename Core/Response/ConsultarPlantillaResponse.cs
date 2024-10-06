using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class ConsultarPlantillaResponse
    {
        public string Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Nombre { get; set; }
        public string Asunto { get; set; }
        public string TipoPlantilla { get; set; }
        public string Firmante { get; set; }
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public string Cierre { get; set; }
    }
}
