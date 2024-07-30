using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.response
{
    public class FiltroNNAResponse
    {
        public long NoCaso { get; set; }
        public string NombreNNA { get; set; }
        public string NoDocumento { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string AgenteAsignado { get; set; }
        public int EstadoId { get; set; }
        public string Estado { get; set; }
    }
}
