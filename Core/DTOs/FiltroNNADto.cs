using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class FiltroNNADto
    {
        public long? NoCaso { get; set; }
        public string? NombreNNA { get; set; }
        public string? NoDocumento { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
        public string? AgenteAsignado { get; set; }
        public int? EstadoId { get; set; }
        public string? Estado { get; set; }
        public string? EstadoDescripcion { get; set; }
        public string? EstadoColorBG { get; set; }
        public string? EstadoColorText { get; set; }
    }
}
