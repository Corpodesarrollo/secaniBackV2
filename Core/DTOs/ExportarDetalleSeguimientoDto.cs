using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ExportarDetalleSeguimientoDto
    {
        public string Nombre { get; set; }
        public DateTime? FechaNacimiento {  get; set; }
        public string? Diagnostico {  get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public long Id { get; set; }
    }
}
