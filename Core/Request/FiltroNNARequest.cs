using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class FiltroNNARequest
    {
        public int? Estado {  get; set; }
        public string? Agente { get; set; }
        public string? Buscar { get; set; }
        public int? Orden {  get; set; }
    }
}
