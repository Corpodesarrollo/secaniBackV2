using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ExportarDetalleSeguimientoContactoDto
    {
        public string Nombre { get; set; }
        public int? Parentesco {  get; set; }
        public string CorreoElectronico {  get; set; }
        public string Telefono {  get; set; }
    }
}
