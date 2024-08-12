using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Request
{
    public class SetSeguimientoRequest
    {
        public int IdNNA {  get; set; }
        public DateTime FechaSeguimiento { get; set; }
        public int IdEstado {  get; set; }
        public int IdContactoNNA {  get; set; }
        public string Telefono {  get; set; }
        public string IdUsuario {  get; set; }
        public long? IdSolicitante {  get; set; }
        public string? ObservacionSolicitante { get; set; }
        public string? IdUsuarioCreacion {get; set; }


    }
}
