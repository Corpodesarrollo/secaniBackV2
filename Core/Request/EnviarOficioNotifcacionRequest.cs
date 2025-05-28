using Core.DTOs;

namespace Core.Request
{
    public class EnviarOficioNotifcacionRequest
    {
        public int IdEntidad { get; set; }
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
