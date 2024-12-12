using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteInconsistenciaPersonaController : ControllerBase
    {
        private readonly IReporteInconsistenciaPersonaService _reporteService;

        public ReporteInconsistenciaPersonaController(IReporteInconsistenciaPersonaService reporteService)
        {
            _reporteService = reporteService;
        }

        /// <summary>
        /// Obtiene la lista de reportes de inconsistencias de personas.
        /// </summary>
        /// <returns>Lista de reportes de inconsistencias.</returns>
        [HttpGet]
        public async Task<IActionResult> GetReporteInconsistencias()
        {
            try
            {
                var reportes = await _reporteService.GetReporteInconsistenciasAsync();
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un reporte de inconsistencia por su ID.
        /// </summary>
        /// <param name="id">ID del reporte de inconsistencia.</param>
        /// <returns>Reporte de inconsistencia correspondiente al ID.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReporteInconsistencia(long id)
        {
            try
            {
                var reporte = await _reporteService.GetReporteInconsistenciaAsync(id);

                if (reporte == null)
                {
                    return NotFound(new { message = $"Reporte con ID {id} no encontrado." });
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
