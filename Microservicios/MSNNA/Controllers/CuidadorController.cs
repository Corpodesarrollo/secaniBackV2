using Core.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    public class CuidadorController(ICuidadorRepo repo) : BaseController
    {
        [HttpPut("SetCuidador/{id}")]
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
    }
}