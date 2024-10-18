namespace Core.Request
{
    public class UploadFileRequest
    {
        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
    }
}
