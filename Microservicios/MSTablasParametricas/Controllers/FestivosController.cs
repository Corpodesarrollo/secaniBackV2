using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FestivosController : ControllerBase
    {
        private readonly IFestivoService _service;
        private readonly IHistoricoTransaccionService _historicoService;

        public FestivosController(IFestivoService service, IHistoricoTransaccionService historicoService)
        {
            _service = service;
            _historicoService = historicoService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FestivoDTO>> GetById(long id, CancellationToken cancellationToken)
        {
            var entity = await _service.GetByIdAsync(id, cancellationToken);
            return entity == null ? NotFound() : Ok(entity);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _service.GetAllAsync(cancellationToken);
            return Ok(entities);
        }

        [HttpGet("NotDeleted")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> GetAllNotDeleted(CancellationToken cancellationToken)
        {
            var entities = await _service.GetAllNotDeletedAsync(cancellationToken);
            return Ok(entities);
        }

        [HttpPost]
        public async Task<ActionResult<FestivoDTO>> Add([FromBody] CreateFestivoRequest request, CancellationToken cancellationToken)
        {
            if (request.HoraInicio >= request.HoraFin)
                return BadRequest("La hora de inicio debe ser menor que la hora de fin.");

            var (success, created) = await _service.CreateAsync(request, cancellationToken);
            if (!success || created == null)
                return BadRequest("No se pudo crear el festivo.");

            var historico = new HistoricoTransaccion
            {
                Id = Guid.NewGuid().ToString(),
                NombreTabla = nameof(TPFestivos),
                FechaTransaccion = DateTime.UtcNow,
                Transaccion = "Adicionar",
                UsuarioId = User?.Identity?.Name ?? "Sistema",
                RegistroAnterior = string.Empty,
                RegistroNuevo = JsonSerializer.Serialize(created),
                Comentario = string.Empty
            };
            await _historicoService.GuardarHistoricoAsync(historico, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateFestivoRequest request, CancellationToken cancellationToken)
        {
            if (id != request.Id)
                return BadRequest("El ID en la URL no coincide con el del cuerpo.");

            if (request.HoraInicio >= request.HoraFin)
                return BadRequest("La hora de inicio debe ser menor que la hora de fin.");

            var antes = await _service.GetByIdAsync(id, cancellationToken);
            var (success, updated) = await _service.UpdateAsync(request, cancellationToken);

            if (!success || updated == null)
                return NotFound("No se pudo actualizar el festivo.");

            var historico = new HistoricoTransaccion
            {
                Id = Guid.NewGuid().ToString(),
                NombreTabla = nameof(TPFestivos),
                FechaTransaccion = DateTime.UtcNow,
                Transaccion = "Actualizacion",
                UsuarioId = User?.Identity?.Name ?? "Sistema",
                RegistroAnterior = JsonSerializer.Serialize(antes),
                RegistroNuevo = JsonSerializer.Serialize(updated),
                Comentario = string.Empty
            };
            historico.Comentario = historico.ObtenerCamposModificados();
            await _historicoService.GuardarHistoricoAsync(historico, cancellationToken);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var entityAntes = await _service.GetByIdAsync(id, cancellationToken);
            var result = await _service.DeleteAsync(id, cancellationToken);

            var historico = new HistoricoTransaccion
            {
                Id = Guid.NewGuid().ToString(),
                NombreTabla = nameof(TPFestivos),
                FechaTransaccion = DateTime.UtcNow,
                Transaccion = "Eliminar",
                UsuarioId = User?.Identity?.Name ?? "Sistema",
                RegistroAnterior = JsonSerializer.Serialize(entityAntes),
                RegistroNuevo = string.Empty,
                Comentario = string.Empty
            };
            await _historicoService.GuardarHistoricoAsync(historico, cancellationToken);

            return Ok(result);
        }

        [HttpGet("EsFestivo/{date}")]
        public async Task<ActionResult<bool>> EsFestivo(DateOnly date, CancellationToken cancellationToken)
        {
            var (success, entity) = await _service.EsFestivoAsync(date, cancellationToken);
            return success;
        }

        [HttpGet("FestivosByAno/{ano}")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> FestivosByAno(int ano, CancellationToken cancellationToken)
        {
            var entities = await _service.GetFestivosByAnoAsync(ano, cancellationToken);
            return Ok(entities);
        }

        [HttpGet("FestivosByAnoMes/{ano}/{mes}")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> FestivosByAnoMes(int ano, int mes, CancellationToken cancellationToken)
        {
            var entities = await _service.GetFestivosByAnoAndMesAsync(ano, mes, cancellationToken);
            return Ok(entities);
        }

        [HttpGet("GetFestivoByDate/{date}")]
        public async Task<ActionResult<FestivoDTO?>> GetFestivoByDate(DateOnly date, CancellationToken cancellationToken)
        {
            var entity = await _service.GetFestivoByDateAsync(date, cancellationToken);
            return Ok(entity);
        }
    }
}
