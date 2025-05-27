using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{

    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class EAPBController(IEAPBRepo service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<TPEAPBDto>>> GetEAPB(CancellationToken cancellationToken)
        {
            var result = await service.GetEAPB();
            return Ok(result);
        }

        [HttpGet("Entidades")]
        public async Task<ActionResult> Entidates(CancellationToken cancellationToken)
        {
            var result = await service.Entidates();
            return Ok(result);
        }
    }
}
