using Core.Authorization;
using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Services.AusenciasUsuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSSeguimiento.Api.Extensions;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AusenciasController : ControllerBase
    {
        private readonly IAusenciasService _service;

        public AusenciasController(IAusenciasService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Crea una ausencia. Reglas: Fecha futura (mínimo 1 día) y no duplicada por UsuarioId+Fecha.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesResponseType(typeof(AusenciaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateAsync([FromBody] AusenciaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var result = await _service.CreateAsync(dto, ct);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza una ausencia. (Opcional: puedes hacer cumplir regla de fecha futura en el repositorio)
        /// </summary>
        [HttpPut("{id:long}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesResponseType(typeof(AusenciaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync([FromRoute] long id, [FromBody] AusenciaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (dto is null)
                return BadRequest(new[] { new { Code = "ARG_NULL", Message = "El cuerpo no puede ser nulo." } });

            if (dto.Id == 0) dto.Id = id;
            if (dto.Id != id)
                return BadRequest(new[] { new { Code = "ID_MISMATCH", Message = "El Id de la ruta no coincide con el del cuerpo." } });

            var result = await _service.UpdateAsync(dto, ct);
            return result.ToActionResult();
        }

        /// <summary>
        /// Lista todas las ausencias.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AusenciaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return result.ToActionResult();
        }

        /// <summary>
        /// Lista ausencias por UsuarioId.
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IReadOnlyList<AusenciaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllByUsuarioIdAsync([FromRoute] string usuarioId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return BadRequest(new[] { new { Code = "USUARIO_REQUERIDO", Message = "UsuarioId es obligatorio." } });

            var result = await _service.GetAllByUsuarioIdAsync(usuarioId, ct);
            return result.ToActionResult();
        }

        /// <summary>
        /// Verifica si existe una ausencia para UsuarioId y Fecha (solo fecha). 
        /// Ejemplo: GET /api/ausencias/exists?usuarioId=U1&fecha=2025-09-21
        /// </summary>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExistsByFechaAndUsuarioIdAsync([FromQuery] string usuarioId, [FromQuery] DateTime fecha, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return BadRequest(new[] { new { Code = "USUARIO_REQUERIDO", Message = "UsuarioId es obligatorio." } });

            if (fecha == default)
                return BadRequest(new[] { new { Code = "FECHA_REQUERIDA", Message = "Debe suministrar la fecha en el querystring (yyyy-MM-dd)." } });

            var result = await _service.ExistsByFechaAndUsuarioIdAsync(usuarioId, fecha, ct);
            return result.ToActionResult();
        }

        /// <summary>
        /// Elimina definitivamente una ausencia por Id. Solo permitido si FechaAusencia es futura.
        /// </summary>
        [HttpDelete("{id:long}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByIdAsync([FromRoute] long id, CancellationToken ct)
        {
            var result = await _service.DeleteByIdAsync(id, ct);
            return result.ToActionResult();
        }
    }
}