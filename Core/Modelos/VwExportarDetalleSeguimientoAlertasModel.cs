using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modelos
{
    [Table("VwExportarDetalleSeguimientoAlertas")]
    public class VwExportarDetalleSeguimientoAlertasModel
    {
        [Key]
        public long AlertaId { get; set; }
        public long SeguimientoId { get; set; }
        public string? FechaNotificacion { get; set; }
        public string? Categoria { get; set; }
        public string? SubCategoriaAlerta { get; set; }
        public string? Observaciones { get; set; }
        public string? Estado { get; set; }
    }
}
