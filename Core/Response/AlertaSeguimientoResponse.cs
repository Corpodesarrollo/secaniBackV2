using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class AlertaSeguimientoResponse
    {
        public long AlertaId { get; set; }
        public long SeguimientoId { get; set; }
        public string? Observaciones { get; set; }
        public int EstadoId { get; set; }
        public DateTime UltimaFechaSeguimiento { get; set; }
        public string NombreAlerta {  get; set; }
    }
}
