namespace Core.DTOs
{
    public class AttachmentFileDto
    {
        public string? FileName { get; set; }
        public string? FileExtension { get; set; }
        public MemoryStream? File { get; set; }
    }
}
