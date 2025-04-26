using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{

    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class ETController(IEAPBRepo service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<TPEAPBDto>>> GetET(CancellationToken cancellationToken)
        {
            var result = await service.GetET();
            return Ok(result);
        }
    }
}
