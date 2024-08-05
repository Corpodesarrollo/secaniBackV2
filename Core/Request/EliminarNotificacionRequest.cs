using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class EliminarNotificacionRequest
    {
        public int IdNotificacionUsuario { get; set; }
        public string IdUsuario { get; set; }
    }
}
