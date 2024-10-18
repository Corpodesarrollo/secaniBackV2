using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class CrearPlantillaCorreoRequest
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Asunto { get; set; }
        public string TipoPlantilla { get; set; }
        public string Firmante { get; set; }
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public string Cierre { get; set; }
        public string? Comentario {  get; set; }
        public string IdUsuario {  get; set; }
        public string Rol {  get; set; }
    }
}
