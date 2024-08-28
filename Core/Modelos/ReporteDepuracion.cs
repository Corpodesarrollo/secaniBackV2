using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modelos
{
    public class ReporteDepuracion
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public int RegistrosIngresados { get; set; }
        public int RegistrosNuevos { get; set; }
        public int RegistrosDuplicados { get; set; }
        public string Estado { get; set; }
        public int SegundasNeoplasias { get; set; }
        public int Recaidas { get; set; }
    }
}
