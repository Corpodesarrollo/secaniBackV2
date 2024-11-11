using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ExportarDetalleSeguimientoSeguimientoAnteriorDto
    {
        public string Id {  get; set; }
        public DateTime Fecha { get; set; }
        public string Asunto {  get; set; }
        public string Observacion {  get; set; }
    }
}
