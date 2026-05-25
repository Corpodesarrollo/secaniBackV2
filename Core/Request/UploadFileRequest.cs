using System.ComponentModel.DataAnnotations;

namespace Core.Request
{
    public class UploadFileRequest
    {
        [Required]
        public byte[]? FileBytes { get; set; }

        [Required]
        public string? FileName { get; set; }

        public string? Extension
        {
            get
            {
                if (FileBytes == null)
                    return null;

                var extension = Path.GetExtension(FileName);
                return extension;
            }
        }
    }
}
