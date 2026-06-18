using Core.Common;
using Core.Request;
using Core.Services.StorageService;
using Microsoft.AspNetCore.Mvc;


namespace MSAuthentication.Api.Controllers
{
    public class StorageController(IStorageService service) : BaseController
    {
        // Bug 2026-06-17: antes retornaba ActionResult<byte[]?> que ASP.NET serializa como
        // JSON base64. El frontend lo guardaba como blob crudo -> archivo corrupto (Excel/PDF
        // rechazaban). Devolver FileContentResult con bytes binarios y nombre del archivo.
        [HttpGet("{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var bytes = await service.DownloadFileAsync(fileName);
            if (bytes == null || bytes.Length == 0) return NotFound();
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost]
        public async Task<ActionResult<bool>> Post(UploadFileRequest request)
        {
            return await service.UploadFileAsync(request.FileBytes, request.FileName);
        }

        [HttpPut]
        public async Task<ActionResult<bool>> Put(UploadFileRequest request)
        {
            return await service.UpdateFileAsync(request.FileBytes, request.FileName);
        }

        [HttpDelete("{fileName}")]
        public async Task<ActionResult<bool>> Delete(string fileName)
        {
            return await service.DeleteFileAsync(fileName);
        }
    }
}
