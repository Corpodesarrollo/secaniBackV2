using Core.Common;
using Core.DTOs;
using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    public class ReporteInconsistenciaPersonaController : BaseController
    {
        private readonly IReporteInconsistenciaPersonaService _reporteService;

        public ReporteInconsistenciaPersonaController(IReporteInconsistenciaPersonaService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpPost]
        public async Task<ActionResult<ReporteInconsistenciaPersonaDTO>> AddReporteInconsistenciaAsync([FromBody] NNADto menor)
        {
            var resultado = await _reporteService.AddReporteInconsistenciaAsync(menor);
            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene la lista de reportes de inconsistencias de personas.
        /// </summary>
        /// <returns>Lista de reportes de inconsistencias.</returns>
        [HttpGet("GetListaInconsistencias")]
        public async Task<IActionResult> GetListaInconsistencias()
        {
            try
            {
                var reportes = await _reporteService.GetReporteInconsistenciasPersonaAsync();
                return Ok(reportes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene reporte de inconsistencias de personas.
        /// </summary>
        /// <returns>Lista de reportes de inconsistencias.</returns>
        [HttpGet("GetReporteInconsistencias")]
        public async Task<IActionResult> GetReporteInconsistencias([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                var reportes = await _reporteService.GetReporteInconsistenciasAsync(fechaInicio, fechaFin);
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
        /// <summary>
        /// Marcar reporte de inconsistencia como resuelto (KPI 6 + KPI 7 Manual).
        /// </summary>
        [HttpPut("{id}/Resolver")]
        public async Task<IActionResult> MarcarResuelto(long id, [FromQuery] string? userId)
        {
            try
            {
                var ok = await _reporteService.MarcarResueltoAsync(id, userId);
                if (!ok) return NotFound(new { message = $"Reporte {id} no encontrado." });
                return Ok(new { message = "Resuelto" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReporteInconsistencia(long id)
        {
            try
            {
                var reporte = await _reporteService.GetReporteInconsistenciaPersonaByIdAsync(id);

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
