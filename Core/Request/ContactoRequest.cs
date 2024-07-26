using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class ContactoRequest
    {
        public string Nombre { get; set; }
        public int IdParentesco { get; set; }
        public string Telefono1 { get; set; }
        public string Telefono2 { get; set; }
    }
}
