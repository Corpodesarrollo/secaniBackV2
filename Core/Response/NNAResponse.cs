using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class NNAResponse
    {
        public long Id {  get; set; }
        public string NombreCompleto {  get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Diagnostico {  get; set; }
    }
}
