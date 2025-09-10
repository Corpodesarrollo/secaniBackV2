using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class CategoriaAlertaController : GenericController<TPCategoriaAlerta, CategoriaAlertaDTO>
    {
        private readonly ICategoriaAlertaService _service;
        private readonly IHistoricoTransaccionService _historicoService;

        public CategoriaAlertaController(ICategoriaAlertaService service, IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
            _service = service;
            _historicoService = historicoService;
        }

        [HttpGet("Subcategorias/{categoriaId}")]
        public async Task<ActionResult<CategoriaAlertaDTO>> CategoriaWithSubcategorias(int categoriaId, CancellationToken cancellationToken)
        {
            return await _service.GetCategoriaAlertaWithSubCategorias(categoriaId, cancellationToken);

        }
    }
}