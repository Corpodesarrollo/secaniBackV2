using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    public class ETController(IEAPBRepo service) : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<List<TPEAPBDto>>> GetET(CancellationToken cancellationToken)
        {
            var result = await service.GetET();
            return Ok(result);
        }
    }
}
