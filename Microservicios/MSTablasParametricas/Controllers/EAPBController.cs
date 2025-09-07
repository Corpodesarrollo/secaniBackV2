using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    public class EAPBController(IEAPBRepo service) : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<List<TPEAPBDto>>> GetEAPB(CancellationToken cancellationToken)
        {
            var result = await service.GetEAPB();
            return Ok(result);
        }

        [HttpGet("Search/{cadena}")]
        public async Task<ActionResult<List<TPEAPBDto>>> Search(string cadena, CancellationToken cancellationToken)
        {
            var result = await service.Search(cadena);
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
