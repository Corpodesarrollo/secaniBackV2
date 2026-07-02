using Core.Authorization;
using Core.Common;
using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace MSAuthentication.Api.Controllers
{
    public class PermisosController : BaseController
    {
        private const string NombreTablaAudit = "Permisos";
        private IPermisosRepo _service;
        private readonly IHistoricoTransaccionService _historico;


        public PermisosController(IPermisosRepo service, IHistoricoTransaccionService historico)
        {
            _service = service;
            _historico = historico;
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

        [HttpGet("Auditoria")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Auditoria(CancellationToken cancellationToken)
        {
            var historico = await _historico.GetHistoricoByTablaAsync(NombreTablaAudit, cancellationToken);
            return Ok(historico);
        }

        [HttpPost]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Add(PermisoRequestDTO dto)
        {
            var (success, response) = await _service.AddAsync(dto, cancellationToken: default);
            await GuardarAuditoria("Adicionar", anterior: null, nuevo: response);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Update(PermisoResponseDTO dto)
        {
            // dto.Id=0 cuando UpdateAsync hara INSERT (upsert BUG-LZ-023). Skip GetByIdAsync(0) que causa NPE.
            var antes = dto.Id > 0
                ? await _service.GetByIdAsync(dto.Id, cancellationToken: default)
                : null;
            await _service.UpdateAsync(dto, cancellationToken: default);
            await GuardarAuditoria("Actualizacion", anterior: antes, nuevo: dto, calcularCamposModificados: true);
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
            await GuardarAuditoria("Eliminar", anterior: entity, nuevo: null);
            return NoContent();
        }

        private async Task GuardarAuditoria(string transaccion, object? anterior, object? nuevo, bool calcularCamposModificados = false)
        {
            try
            {
                var historico = new HistoricoTransaccion
                {
                    Id = Guid.NewGuid().ToString(),
                    NombreTabla = NombreTablaAudit,
                    FechaTransaccion = DateTime.UtcNow,
                    Transaccion = transaccion,
                    UsuarioId = User?.Identity?.Name ?? "Sistema",
                    RegistroAnterior = anterior != null ? JsonSerializer.Serialize(anterior) : string.Empty,
                    RegistroNuevo = nuevo != null ? JsonSerializer.Serialize(nuevo) : string.Empty,
                    Comentario = string.Empty
                };
                if (calcularCamposModificados)
                    historico.Comentario = historico.ObtenerCamposModificados();
                await _historico.GuardarHistoricoAsync(historico, cancellationToken: default);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WARN audit Permisos {transaccion} fallo: {ex.Message}");
            }
        }
    }
}
