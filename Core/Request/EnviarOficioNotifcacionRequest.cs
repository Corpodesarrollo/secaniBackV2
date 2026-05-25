using Core.DTOs;

namespace Core.Request
{
    public class EnviarOficioNotifcacionRequest
    {
        // BUG-LZ-074: frontend envia codigo (string) en idEntidad pero backend exigia int -> 400
        // BadRequest validacion ASP.NET. Cambiar a string? (el handler EnviarOficioNotificacion
        // no usa este campo directamente, solo el IdNotificacion para lookup).
        public string? IdEntidad { get; set; }
        public int IdNotificacion { get; set; }
        public string[]? Para { get; set; }
        public string[]? ConCopia { get; set; }
        public long? PlantillaId { get; set; }
        public string? Asunto { get; set; }
        public string? Mensaje { get; set; }
        public string? Enlace { get; set; }
        public ArchivoDto? Adjunto { get; set; }
        public string? Comentario { get; set; }
        public string? Firma { get; set; }

    }
}
