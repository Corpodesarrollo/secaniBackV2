using Core.Authorization;
using Core.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    public class CuidadorController(ICuidadorRepo repo) : BaseController
    {
        [HttpPut("SetCuidador/{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<ActionResult> SetUserCuidador(string id)
        {
            try
            {
                var result = await repo.SetUserCuidador(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetAllCuidadores()
        {
            try
            {
                var result = await repo.GetAllCuidadores();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("DownloadManualCuidador")]
        public IActionResult DownloadManualCuidador()
        {
            try
            {
                var filePath = "Resources/Manual_Cuidador.pdf";
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileName = "Manual_Cuidador.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}