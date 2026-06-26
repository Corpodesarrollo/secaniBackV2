using Core.Authorization;
using Core.Common;
using Core.CQRS.MSUsuariosyRoles.Commands.Role;
using Core.CQRS.MSUsuariosyRoles.Queries.Role;
using Core.DTOs.MSUsuariosyRoles;
using Core.Interfaces.Services.MSTablasParametricas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace MSAuthentication.Api.Controllers
{
    public class RoleController : BaseController
    {
        private const string NombreTablaAudit = "Roles";
        public readonly IMediator _mediator;
        private readonly IHistoricoTransaccionService _historico;

        public RoleController(IMediator mediator, IHistoricoTransaccionService historico)
        {
            _mediator = mediator;
            _historico = historico;
        }

        [HttpPost("Create")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> CreateRoleAsync(RoleCreateCommand command)
        {
            var result = await _mediator.Send(command);
            await GuardarAuditoria("Adicionar", anterior: null, nuevo: command);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        [ProducesDefaultResponseType(typeof(List<RoleResponseDTO>))]
        public async Task<IActionResult> GetRoleAsync()
        {
            return Ok(await _mediator.Send(new GetRoleQuery()));
        }


        [HttpGet("{id}")]
        [ProducesDefaultResponseType(typeof(RoleResponseDTO))]
        public async Task<IActionResult> GetRoleByIdAsync(string id)
        {
            return Ok(await _mediator.Send(new GetRoleByIdQuery() { RoleId = id }));
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            var antes = await _mediator.Send(new GetRoleByIdQuery() { RoleId = id });
            var result = await _mediator.Send(new DeleteRoleCommand()
            {
                RoleId = id
            });
            await GuardarAuditoria("Eliminar", anterior: antes, nuevo: null);
            return Ok(result);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> EditRole(string id, [FromBody] UpdateRoleCommand command)
        {
            if (id == command.Id)
            {
                var antes = await _mediator.Send(new GetRoleByIdQuery() { RoleId = id });
                var result = await _mediator.Send(command);
                await GuardarAuditoria("Actualizacion", anterior: antes, nuevo: command, calcularCamposModificados: true);
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("Auditoria")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Auditoria(CancellationToken cancellationToken)
        {
            var historico = await _historico.GetHistoricoByTablaAsync(NombreTablaAudit, cancellationToken);
            return Ok(historico);
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
                Console.Error.WriteLine($"WARN audit Roles {transaccion} fallo: {ex.Message}");
            }
        }

    }
}
