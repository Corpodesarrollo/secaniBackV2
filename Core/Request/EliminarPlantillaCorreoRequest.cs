using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class EliminarPlantillaCorreoRequest
    {
        public string Id {  get; set; }
        public string Comentario { get; set; }
        public string IdUsuario {  get; set; }  
        public string Rol { get; set; }
    }
}
