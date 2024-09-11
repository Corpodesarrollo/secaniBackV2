using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class NotificacionEntidadPlantilla
    {
        public string CiudadEnvio { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Membrete { get; set; }
        public string NombreEntidad { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string DescripcionAlerta { get; set; }
        public string ObservacionAlerta { get; set; }
        public string ComentarioNotificacion { get; set; }
        public string PrimerNombreNNA { get; set; }
        public string SegundoNombreNNA { get; set; }
        public string PrimerApellidoNNA { get; set; }
        public string SegundoApellidoNNA { get; set; }
        public string DocumentoNNA { get; set; }
        public int EdadNNA { get; set; }
        public string DiagnosticoNNA { get; set; }
        public string TelefonoAcudienteNNA { get; set; }
        public string Cierre { get; set; }
        public string Firma { get; set; }
        public DateTime? FechaNacimientoNNA { get; set; }
    }
}
