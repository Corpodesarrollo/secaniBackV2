using Core.Authorization;
using Core.Common;
using Core.DTOs.MSPermisos;
using Core.Services.MSPermisos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSAuthentication.Api.Controllers
{
    public class ModulosController(IModuloService service) : BaseController
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
            var result = await _service.GetAllByIdPadreAsync(0, cancellationToken: default);
            var filteredResult = result.Where(x => !x.ModuloComponenteObjetoIdPadre.HasValue || x.ModuloComponenteObjetoIdPadre == 0);

            return Ok(filteredResult);
        }

        [HttpGet("{idPadre}")]
        public async Task<IActionResult> Modulos(int idPadre)
        {
            var items = await _service.GetAllByIdPadreAsync(idPadre, cancellationToken: default);
            return Ok(items);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Update(ModuloResponseDTO dto)
        {
            await _service.UpdateAsync(dto, cancellationToken: default);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
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
