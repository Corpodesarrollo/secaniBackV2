using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class FinalizarSeguimientoRequest
    {
        public int IdSegumiento {  get; set; }
        public DateTime? FechaDefuncion { get; set; }
        public string? CausaMuerte {  get; set; }
        public string? Observacion {  get; set; }
        public List<AlertaRequest> alertas { get; set; }

    }
}
