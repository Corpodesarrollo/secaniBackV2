namespace Core.DTOs
{
    public class EnviarRespuestaDto
    {
        public long Id { get; set; }
        public string[]? Para { get; set; }
        public string[]? Cc { get; set; }
        public string? Asunto { get; set; }
        public string? Mensaje { get; set; }
        public string? Firma { get; set; }
        public AttachmentFileDto? Archivo { get; set; }
    }
}
