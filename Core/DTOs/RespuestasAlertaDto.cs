namespace Core.DTOs
{
    public class RespuestasAlertaDto
    {
        public long Id { get; set; }
        public long NotificacionEntidadId { get; set; }
        public string? Nombre { get; set; }
        public string? Cargo { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Respuesta { get; set; }
        public long EntidadId { get; set; }
        public string? Asunto { get; set; }
        public string? ConCopia { get; set; }
        public string? Firma { get; set; }
        public string? Mensaje { get; set; }
        public string? Para { get; set; }
        public string? De { get; set; }
        public long IdAlerta { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
