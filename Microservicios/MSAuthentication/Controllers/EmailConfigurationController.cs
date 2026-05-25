using Core.Common;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace MSAuthentication.Api.Controllers
{
    public class EmailConfigurationController(IEmailConfigurationRepo service) : BaseController
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
