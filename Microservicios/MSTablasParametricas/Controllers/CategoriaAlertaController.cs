using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class CategoriaAlertaController : GenericController<TPCategoriaAlerta, CategoriaAlertaDTO>
    {
        private readonly ICategoriaAlertaService _service;

        public CategoriaAlertaController(ICategoriaAlertaService service) : base(service)
        {
            _service = service;
        }

        [HttpGet("Subcategorias/{categoriaId}")]
        public async Task<ActionResult<CategoriaAlertaDTO>> CategoriaWithSubcategorias(int categoriaId, CancellationToken cancellationToken)
        {
            return await _service.GetCategoriaAlertaWithSubCategorias(categoriaId, cancellationToken);

        }
    }
}