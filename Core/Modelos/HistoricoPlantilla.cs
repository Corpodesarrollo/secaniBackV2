using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modelos
{
    [Table("HistoricoPlantilla")]
    public class HistoricoPlantilla
    {
        public string Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Transaccion { get; set; }
        public string UsuarioOrigen { get; set; }
        public string UsuarioRol { get; set; }
        public string Comentario { get; set; }
    }
}
