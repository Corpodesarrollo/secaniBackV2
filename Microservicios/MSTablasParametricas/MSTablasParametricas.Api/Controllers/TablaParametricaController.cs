using Core.Modelos.Common;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablaParametricaController : ControllerBase
    {
        private readonly TablaParametricaService _service;

        public TablaParametricaController(TablaParametricaService service)
        {
            _service = service;
        }

        [HttpGet("{nomTREF}")]
        public async Task<ActionResult<List<TPExternalEntityBase>>> Get(string nomTREF, CancellationToken cancellationToken)
        {
            var result = await _service.GetBynomTREF(nomTREF, CancellationToken.None);
            return Ok(result);
        }

        [HttpGet("Municipios/{CodDepto}")]
        public async Task<ActionResult<List<TPExternalEntityBase>>> GetMunicipios(string CodDepto, CancellationToken cancellationToken)
        {
            var result = await _service.GetMunicipiosByDepto(CodDepto, cancellationToken);
            return Ok(result);
        }
    }
}

