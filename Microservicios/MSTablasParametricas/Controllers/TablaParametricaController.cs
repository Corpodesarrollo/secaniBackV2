using Core.Modelos.Common;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    [AutoValidateAntiforgeryToken]
    public class TablaParametricaController : ControllerBase
    {
        private readonly TablaParametricaService _service;

        public TablaParametricaController(TablaParametricaService service)
        {
            _service = service;
        }

        [HttpGet("{NombreTabla}")]
        public async Task<ActionResult<List<TPExternalEntityBase>>> Get(string NombreTabla, CancellationToken cancellationToken)
        {
            var result = await _service.GetBynomTREF(NombreTabla, CancellationToken.None);
            return Ok(result);
        }

        [HttpGet("{NombreTabla}/{Codigo}")]
        public async Task<ActionResult<List<TPExternalEntityBase>>> GetFilter(string NombreTabla, int Codigo, CancellationToken cancellationToken)
        {
            var result = await _service.GetBynomTREFCodigo(NombreTabla, Codigo, CancellationToken.None);
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

