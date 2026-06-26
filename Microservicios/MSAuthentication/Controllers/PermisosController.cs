using Core.Authorization;
using Core.Common;
using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios;
using Core.request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MSAuthentication.Api.Controllers
{
    public class PermisosController : BaseController
    {
        private IPermisosRepo _service;


        public PermisosController(IPermisosRepo service)
        {
            _service = service;
        }

        [HttpPost("MenuXRolId")]
        public IActionResult MenuXRolId(GetVwMenuRequest request)
        {
            var result = _service.MenuXRolId(request, cancellationToken: default);

            return Ok(result);
        }

        [HttpGet("MenuXRolId")]
        public IActionResult Options2()
        {
            return Ok();
        }

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

        [HttpGet("GetByRoleId/{id}")]
        public async Task<IActionResult> GetByRoleId(string id)
        {
            var result = await _service.GetAllByRoleIdAsync(id, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("GetByModuloId/{id}")]
        public async Task<IActionResult> GetByModuloId(int id)
        {
            var result = await _service.GetAllByModuloIdAsync(id, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("GetByModuloPadreId/{id}")]
        public async Task<IActionResult> GetByModuloPadreId(int id)
        {
            var result = await _service.GetAllByModuloPadreIdAsync(id, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("GetByRoleandModuloId/{roleId}/{moduloId}")]
        public async Task<IActionResult> GetByRoleandModuloId(string roleId, int moduloId)
        {
            var result = await _service.GetAllByModuloandRoleAsync(roleId, moduloId, cancellationToken: default);
            return Ok(result);
        }

        [HttpGet("CansByPathAndRoleId/{path}/{roleId}")]
        public async Task<IActionResult> CansByPathAndRoleId(string path, string roleId)
        {
            var result = await _service.CansByPathAndRoleId(path, roleId, cancellationToken: default);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Add(PermisoRequestDTO dto)
        {
            var (success, response) = await _service.AddAsync(dto, cancellationToken: default);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Update(PermisoResponseDTO dto)
        {
            await _service.UpdateAsync(dto, cancellationToken: default);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
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
