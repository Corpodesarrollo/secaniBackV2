using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class AsignacionManualRequest
    {
        public string IdUsuario {  get; set; }
        public string Motivo {  get; set; }
        public List<int> Segumientos { get; set; }
    }
}
