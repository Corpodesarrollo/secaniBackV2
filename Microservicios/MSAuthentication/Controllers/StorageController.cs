using Core.Request;
using Core.Services.StorageService;
using Microsoft.AspNetCore.Mvc;


namespace MSAuthentication.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class StorageController(IStorageService service) : ControllerBase
    {
        [HttpGet("{fileName}")]
        public async Task<ActionResult<byte[]?>> DownloadFile(string fileName)
        {
            return await service.DownloadFileAsync(fileName);
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
