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
        public long Id { get; set; }
        public long IdPlantilla { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Transaccion { get; set; }
        public string UsuarioOrigen { get; set; }
        public string UsuarioRol { get; set; }
        public string Comentario { get; set; }
        public string RegistroAnterior { get; set; }
        public string RegistroNuevo { get; set; }
    }
}
