using Core.DTOs.MSPermisos;
using Core.Services.MSPermisos;
using Microsoft.AspNetCore.Mvc;

namespace MSPermisos.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisosController(IPermisoService service) : ControllerBase
    {
        private readonly IPermisoService _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("Role/{roleid}")]
        public async Task<IActionResult> GetAllByRoleId(string roleid)
        {
            var result = await _service.GetAllByRoleIdAsync(roleid, cancellationToken: default);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PermisoRequestDTO dto)
        {
            var (success, response) = await _service.AddAsync(dto, cancellationToken: default);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(PermisoResponseDTO dto)
        {
            await _service.UpdateAsync(dto, cancellationToken: default);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
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