using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class HistoricoPlantillaCorreoResponse
    {
        public string Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Transaccion { get; set; }
        public string UsuarioOrigen { get; set; }
        public string UsuarioRol { get; set; }
        public string Comentario { get; set; }
    }
}
