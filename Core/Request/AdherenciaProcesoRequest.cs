using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class AdherenciaProcesoRequest
    {
        public bool HaDejadoTratamiento { get; set; }
        public int TiempoDejadoTratamiento { get; set; }
        public int IdUnidadTiempoDejadoTratamiento { get; set; }
        public int IdCausaInasistenciaTratamiento { get; set; }
        public string OtraCausaDejadoTratamiento { get; set; }
        public bool EstudiaActualmente { get; set; }
        public bool HaDejadoEstudiar { get; set; }
        public int CuantoTiempoDejadoEstudiar { get; set; }
        public int IdUnidadTiempoDejadoEstudiar { get; set; }
        public bool InformacionClara { get; set; }
        public string Observacion {  get; set; }
        public int IdSeguimiento { get; set; }
    }
}
