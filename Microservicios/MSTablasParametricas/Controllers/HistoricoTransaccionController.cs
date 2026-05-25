using Core.Common;
using Core.Interfaces.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    public class HistoricoTransaccionController : BaseController
    {
        private readonly IHistoricoTransaccionService _historicoService;

        // Lista de tablas paramétricas permitidas (insensible a mayúsculas)
        private static readonly HashSet<string> TablasPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TPCategoriaAlerta",
            "TPCausaInasistencia",
            "TPCIE10",
            "TPEstadoAlerta",
            "TPEstadoIngresoEstrategia",
            "TPEstadoNNA",
            "TPEstadoSeguimiento",
            "TPFestivos",
            "TPMalaAtencionIPS",
            "TPMotivoCierreSolicitud",
            "TPOrigenReporte",
            "TPRazonesSinDiagnostico",
            "TPSubCategoriaAlerta",
            "TPTipoFallaLlamada"
        };

        public HistoricoTransaccionController(IHistoricoTransaccionService historicoService)
        {
            _historicoService = historicoService;
        }

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
        /// Obtener la lista de nombres de tablas que guardan histórico.
        /// </summary>
        [HttpGet("nombre-tablas-con-historico")]
        public ActionResult<IEnumerable<string>> GetNombreTablasConHistorico()
        {
            return Ok(TablasPermitidas.OrderBy(t => t).ToList());
        }

        /// <summary>
        /// Obtener todos los registros históricos por nombre de tabla.
        /// </summary>
        [HttpGet("tabla/{nombreTabla}")]
        public async Task<ActionResult<IEnumerable<HistoricoTransaccion>>> GetByTabla(string nombreTabla, CancellationToken cancellationToken)
        {
            // Verificar si el nombre de la tabla es válido (insensible a mayúsculas)
            if (!TablasPermitidas.Contains(nombreTabla))
            {
                return BadRequest(new
                {
                    Message = $"La tabla '{nombreTabla}' no guarda histórico."
                });
            }

            var historicos = await _historicoService.GetHistoricoByTablaAsync(nombreTabla, cancellationToken);

            if (historicos == null || !historicos.Any())
            {
                return Ok(new List<HistoricoTransaccion>());
            }

            return Ok(historicos);
        }
    }
}
