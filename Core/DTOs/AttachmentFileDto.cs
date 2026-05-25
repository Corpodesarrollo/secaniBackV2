namespace Core.DTOs
{
    public class AttachmentFileDto
    {
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public byte[]? File { get; set; }
    }
}
