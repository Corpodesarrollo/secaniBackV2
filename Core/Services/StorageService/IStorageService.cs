


namespace Core.Services.StorageService
{
    public interface IStorageService
    {
        Task<bool> DeleteFileAsync(string fileName);
        Task<byte[]?> DownloadFileAsync(string fileName);
        Task<List<string>> ListFilesAsync();
        Task<List<string>> ListFilesAsync(string prefix);
        Task<bool> UpdateFileAsync(byte[] fileBytes, string fileName);
        Task<bool> UploadFileAsync(byte[] fileBytes, string fileName, bool replace = false);
    }
}
