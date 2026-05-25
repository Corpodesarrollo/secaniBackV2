using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Services.AusenciasUsuario;
using Microsoft.AspNetCore.Mvc;
using MSSeguimiento.Api.Extensions;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("api/horario-laboral")]
    [Produces("application/json")]
    public class HorarioLaboralAgenteController : ControllerBase
    {
        private readonly IHorarioLaboralAgenteService _service;

        public HorarioLaboralAgenteController(IHorarioLaboralAgenteService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // ----------------------------------------------
        // POST: /api/horario-laboral/guardar-dia
        // Crea o actualiza un día (upsert)
        // Body: HorarioLaboralAgenteDto { userId, dia(0..6), horaEntrada, horaSalida, fecha(opc) }
        // ----------------------------------------------
        [HttpPost("guardar-dia")]
        [ProducesResponseType(typeof(HorarioLaboralAgenteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarDiaAsync([FromBody] HorarioLaboralAgenteDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var result = await _service.UpsertDayAsync(dto, ct);
            return result.ToActionResult();
        }

        // ----------------------------------------------
        // POST: /api/horario-laboral/guardar-rango
        // Crea o actualiza un rango de días [diaInicial..diaFinal] (0..6)
        // Body: { userId, diaInicial, diaFinal, horaEntrada, horaSalida }
        // ----------------------------------------------
        public sealed class GuardarRangoRequest
        {
            public string? UserId { get; set; }
            public int DiaInicial { get; set; } // 0..6
            public int DiaFinal { get; set; }   // 0..6
            public TimeSpan HoraEntrada { get; set; }
            public TimeSpan HoraSalida { get; set; }
        }

        [HttpPost("guardar-rango")]
        [ProducesResponseType(typeof(IReadOnlyList<HorarioLaboralAgenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GuardarRangoAsync([FromBody] GuardarRangoRequest body, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (body is null) return BadRequest(new[] { new { Code = "ARG_NULL", Message = "El cuerpo no puede ser nulo." } });
            if (string.IsNullOrWhiteSpace(body.UserId))
                return BadRequest(new[] { new { Code = "USER_REQUERIDO", Message = "UserId es obligatorio.", Field = "UserId" } });

            var result = await _service.UpsertDaysAsync(
                body.UserId!, body.DiaInicial, body.DiaFinal, body.HoraEntrada, body.HoraSalida, ct);

            return result.ToActionResult();
        }

        // ----------------------------------------------
        // DELETE: /api/horario-laboral/eliminar-dia/{userId}/{dia}
        // “Borra” el día poniendo 00:00–00:00
        // ----------------------------------------------
        [HttpDelete("eliminar-dia/{userId}/{dia:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarDiaAsync([FromRoute] string userId, [FromRoute] int dia, CancellationToken ct)
        {
            var result = await _service.DeleteDayAsync(userId, dia, ct);
            return result.ToActionResult();
        }

        // ----------------------------------------------
        // DELETE: /api/horario-laboral/eliminar-rango/{userId}?desde=0&hasta=6
        // “Borra” el rango poniendo 00:00–00:00
        // ----------------------------------------------
        [HttpDelete("eliminar-rango/{userId}")]
        [ProducesResponseType(typeof(IReadOnlyList<HorarioLaboralAgenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EliminarRangoAsync(
            [FromRoute] string userId,
            [FromQuery] int desde,
            [FromQuery] int hasta,
            CancellationToken ct)
        {
            var result = await _service.DeleteDaysAsync(userId, desde, hasta, ct);
            return result.ToActionResult();
        }

        // ----------------------------------------------
        // GET: /api/horario-laboral/obtener-usuario/{userId}
        // Devuelve 7 días (0..6). Si falta alguno, retorna 00:00–00:00.
        // ----------------------------------------------
        [HttpGet("obtener-usuario/{userId}")]
        [ProducesResponseType(typeof(IReadOnlyList<HorarioLaboralAgenteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ObtenerPorUsuarioAsync([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _service.GetScheduleAsync(userId, ct);
            return result.ToActionResult();
        }
    }
}
