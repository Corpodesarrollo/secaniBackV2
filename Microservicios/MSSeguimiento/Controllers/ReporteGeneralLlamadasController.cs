using Core.Common;
using Core.Interfaces.Services.Llamadas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class ReporteGeneralLlamadasController(IResumenLlamadasService service) : BaseController
    {
        private readonly IResumenLlamadasService _service = service;

        /// <summary>
        /// Actualiza las observaciones de un resumen de llamadas.
        /// </summary>
        [HttpPut("ActualizarObservaciones/{id}")]
        public async Task<IActionResult> ActualizarObservaciones(long id, [FromBody] string observaciones)
        {
            var resultado = await _service.ActualizarObservaciones(id, observaciones);
            if (!resultado)
                return NotFound($"No se encontró el resumen de llamadas con Id {id}.");

            return Ok("Observaciones actualizadas correctamente.");
        }

        /// <summary>
        /// Actualiza el resumen y los detalles de llamadas basado en un intento.
        /// </summary>
        [HttpPost("ActualizarResumenYDetalleLlamadas")]
        public async Task<IActionResult> ActualizarResumenYDetalleLlamadasAsync([FromBody] Intentos intento)
        {
            var resultado = await _service.ActualizarResumenYDetalleLlamadasAsync(intento);
            if (!resultado)
                return BadRequest("Error al actualizar el resumen de llamadas.");

            return Ok("Resumen de llamadas actualizado correctamente.");
        }

        /// <summary>
        /// Obtiene el resumen de llamadas entre un rango de fechas.
        /// </summary>
        [HttpGet("GetReporteGeneralLlamadas")]
        public async Task<IActionResult> GetResumenLlamadas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            var resumen = await _service.GetResumenLlamadas(fechaInicio, fechaFin);
            if (resumen == null || resumen.Count == 0)
                return NotFound("No hay registros en el rango de fechas especificado.");

            return Ok(resumen);
        }

        [HttpPost("InicializarResumenLlamadas")]
        public async Task<IActionResult> EjecutarActualizacionDesdeIntentos()
        {
            var resultado = await _service.EjecutarActualizacionDesdeIntentos();

            if (!resultado)
                return BadRequest("Error al ejecutar la actualización.");

            return Ok("Resumen de llamadas actualizado correctamente desde Intentos.");
        }
    }
}
