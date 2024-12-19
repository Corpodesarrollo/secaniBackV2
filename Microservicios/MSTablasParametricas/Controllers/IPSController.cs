using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class IPSController(IIpsRepo service) : Controller
    {

        [HttpGet("{codeMunicipio}")]
        public async Task<ActionResult<TPIPSDto[]>> GetMunicipio(string codeMunicipio)
        {
            var result = await service.GetMunicipio(codeMunicipio);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<TPIPSDto[]>> GetAll()
        {
            var result = await service.GetAll();
            return Ok(result);
        }
    }
}
