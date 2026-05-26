using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    public class IPSController(IIpsRepo service) : BaseController
    {

        [HttpGet("{codeMunicipio}")]
        public async Task<ActionResult<TPIPSDto[]>> GetMunicipio(string codeMunicipio)
        {
            var result = await service.GetMunicipio(codeMunicipio);
            return Ok(result);
        }

        [HttpGet]
        // BUG QA 2026-05-25: cachear 1h en CDN/cliente. El listado IPS habilitadas
        // (~10 MB / ~1 MB gzip) cambia rara vez (cargas REPS programadas).
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<TPIPSSlimDto[]>> GetAll()
        {
            var result = await service.GetAll();
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<TPIPSDto>> GetById(long id)
        {
            var result = await service.GetById(id);
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
