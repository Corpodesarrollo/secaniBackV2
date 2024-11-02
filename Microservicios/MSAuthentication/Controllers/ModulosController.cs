using Core.DTOs.MSPermisos;
using Core.Services.MSPermisos;
using Microsoft.AspNetCore.Mvc;

namespace MSAuthentication.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class ModulosController(IModuloService service) : ControllerBase
    {
        private readonly IModuloService _service = service;

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(cancellationToken: default);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Modulos()
        {
            var result = await _service.GetModulos(cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("{PadreId}")]
        public async Task<IActionResult> ModulosByPadreId(int PadreId)
        {
            var result = await _service.GetModulosByPadreId(PadreId, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken: default);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModuloRequestDTO dto)
        {
            var (success, response) = await _service.AddAsync(dto, cancellationToken: default);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(ModuloResponseDTO dto)
        {
            await _service.UpdateAsync(dto, cancellationToken: default);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _service.GetByIdAsync(id, cancellationToken: default);
            if (entity == null)
            {
                return NotFound();
            }
            await _service.DeleteAsync(entity, cancellationToken: default);
            return NoContent();
        }
    }
}