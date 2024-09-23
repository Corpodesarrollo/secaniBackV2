using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace Core.Services.StorageService
{
    public class StorageService : IStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public StorageService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("StorageConnection") ?? "";
            var containerName = config["StorageContainerName"];

            BlobServiceClient blobServiceClient = new(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<bool> UploadFileAsync(byte[] fileBytes, string fileName, bool replace = false)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);

                using var stream = new MemoryStream(fileBytes);
                await blobClient.UploadAsync(stream, replace); // 'true' para sobrescribir si ya existe

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al subir archivo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateFileAsync(byte[] fileBytes, string fileName)
        {
            try
            {
                await UploadFileAsync(fileBytes, fileName, true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar archivo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
                return false;
            }
        }

        public async Task<byte[]?> DownloadFileAsync(string fileName)
        {
            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(fileName);
                BlobDownloadInfo download = await blobClient.DownloadAsync();

                using var memoryStream = new MemoryStream();
                await download.Content.CopyToAsync(memoryStream);

                return memoryStream.ToArray(); // Retorna el archivo como byte[]
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al descargar archivo: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> ListFilesAsync()
        {
            try
            {
                var files = new List<string>();
                await foreach (var blobItem in _containerClient.GetBlobsAsync())
                    files.Add(blobItem.Name);

                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al listar archivos: {ex.Message}");
                return [];
            }
        }
    }
}
