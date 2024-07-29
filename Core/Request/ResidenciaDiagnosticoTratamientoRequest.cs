using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class ResidenciaDiagnosticoTratamientoRequest
    {
        public ResidenciaRequest residenciaOrigen {  get; set; }
        public ResidenciaRequest? residenciaDestino {  get; set; }
        public int IdSeguimiento { get; set; }
    }
}
