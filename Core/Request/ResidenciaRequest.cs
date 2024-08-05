using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class ResidenciaRequest
    {
        public int IdMunicipio {  get; set; }
        public string Barrio { get; set; }
        public int IdArea { get; set; }
        public string Direccion { get; set; }
        public int IdEstrato { get; set; }
        public string TelefonoFijo {  get; set; }
    }
}
