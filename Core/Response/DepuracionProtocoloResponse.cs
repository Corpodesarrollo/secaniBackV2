using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class DepuracionProtocoloResponse
    {
        public int Nuevos {  get; set; }
        public int Recaidas {  get; set; }
        public int SegundasNeoplasias {  get; set; }
        // BUG-025-ext: registros descartados por edad/defuncion
        public int ExcluidosMayorEdad { get; set; }
        public int ExcluidosFallecidos { get; set; }

        public string Estado {  get; set; }
    }
}
