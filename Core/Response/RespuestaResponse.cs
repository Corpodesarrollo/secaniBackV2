using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Response
{
    public class RespuestaResponse<T>
    {
        public bool? Estado { get; set; } = false;
        public string? Descripcion { get; set; } = "";
        public List<T>? Datos { get; set; } = new List<T>();
    }
}
