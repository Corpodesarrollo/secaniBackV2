using Core.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class SeguimientoNNAResponse
    {
        public long IdSeguimiento {  get; set; }
        public DateTime FechaSeguimiento { get; set; }
        public List<AlertaSeguimientoResponse> alertasSeguimientos {  get; set; }
        public string Observacion {  get; set; }
        public string NombreEntidad {  get; set; }
        public DateTime FechaNotificacion { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string Asunto {  get; set; }
    }
}
