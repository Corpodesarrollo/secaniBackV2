using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class DatosBasicosNNAResponse
    {
        public string NombreCompleto {  get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Diagnostico {  get; set; }
        public DateTime? FechaInicioSegumiento { get; set; }
        public string DiagnosticoId {  get; set; }
    }
}
