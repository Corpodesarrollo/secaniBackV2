using Core.Modelos.Common;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntidadesController : ControllerBase
    {
        private readonly TablaParametricaService _service;

        public EntidadesController(TablaParametricaService service)
        {
            _service = service;
        }

        [HttpGet("Entidades")]
        public async Task<ActionResult<List<TPEntidadExterna>>> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetEntidates(CancellationToken.None);
            return Ok(result);
        }

        [HttpGet("Entidad/{Codigo}")]
        public async Task<ActionResult<TPEntidadExterna>> GetEntidad(string Codigo, CancellationToken cancellationToken)
        {
            var result = await _service.GetEntidadById(Codigo, CancellationToken.None);
            return Ok(result);
        }
    }
}
