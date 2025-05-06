using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace MSAuthentication.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class EmailConfigurationController(IEmailConfigurationRepo service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var resul = await service.Get();
            if (resul == null)
                return NotFound();

            return Ok(resul);
        }
    }
}
