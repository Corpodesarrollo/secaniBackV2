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

        [HttpGet("Search/{cadena}")]
        public async Task<ActionResult<TPIPSDto[]>> Search(string cadena)
        {
            var result = await service.Search(cadena);
            return Ok(result);
        }
    }
}
