using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class EstadoDiagnosticoTratamientoRequest
    {
        public int IdEstado { get; set; }
        public int IdSeguimiento { get; set; }
    }
}
