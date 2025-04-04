using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoricoTransaccionController(IHistoricoTransaccionService historicoService) : ControllerBase
    {
        private readonly IHistoricoTransaccionService _historicoService = historicoService;

        /// <summary>
        /// Obtener todos los registros históricos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HistoricoTransaccion>>> GetAll(CancellationToken cancellationToken)
        {
            var historicos = await _historicoService.GetHistoricosAsync(cancellationToken);
            return Ok(historicos);
        }

        /// <summary>
        /// Obtener todos los registros históricos por nombre de tabla.
        /// </summary>
        [HttpGet("tabla/{nombreTabla}")]
        public async Task<ActionResult<IEnumerable<HistoricoTransaccion>>> GetByTabla(string nombreTabla, CancellationToken cancellationToken)
        {
            var historicos = await _historicoService.GetHistoricoByTablaAsync(nombreTabla, cancellationToken);
            if (historicos == null || !historicos.Any())
                return NotFound();

            return Ok(historicos);
        }
    }
}
