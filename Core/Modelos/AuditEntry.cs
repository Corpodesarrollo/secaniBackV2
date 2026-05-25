using DocumentFormat.OpenXml.Math;
using System.Text.Json;

namespace Core.Modelos
{
    public class AuditEntry
    {
        public string Id { get; set; }
        public string? UserId { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Tabla { get; set; }
        public string Operacion { get; set; }
        public string RegistroAnterior { get; set; }
        public string RegistroNuevo { get; set; }
    }
}
