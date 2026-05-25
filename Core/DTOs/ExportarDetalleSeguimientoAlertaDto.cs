using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ExportarDetalleSeguimientoAlertaDto
    {
        public int IdSeguimiento {  get; set; }
        public DateTime Fecha { get; set; }
        public string Categoria {  get; set; }
        public string Subcategoria {  get; set; }
        public string Entidad {  get; set; }
        public string Estado {  get; set; }
    }
}
