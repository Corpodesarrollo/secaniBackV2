using Core.Authorization;
using Core.Common;
using Core.CQRS.MSUsuariosyRoles.Commands.User;
using Core.CQRS.MSUsuariosyRoles.Queries.User;
using Core.DTOs.MSUsuariosyRoles;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.MSTablasParametricas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace MSAuthentication.Api.Controllers
{
    public class UserController : BaseController
    {
        private const string NombreTablaAudit = "Usuarios";
        private readonly IMediator _mediator;
        private readonly IUsurioRepo _usurioRepo;
        private readonly IHistoricoTransaccionService _historico;

        public UserController(IMediator mediator, IUsurioRepo usurioRepo, IHistoricoTransaccionService historico)
        {
            _mediator = mediator;
            _usurioRepo = usurioRepo;
            _historico = historico;
        }

        [HttpPost("Create")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> CreateUser(CreateUserCommand command)
        {
            var result = await _mediator.Send(command);
            await GuardarAuditoria("Adicionar", anterior: null, nuevo: command);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        [ProducesDefaultResponseType(typeof(List<UserResponseDTO>))]
        public async Task<IActionResult> GetAllUserAsync()
        {
            return Ok(await _mediator.Send(new GetUserQuery()));
        }

        [HttpDelete("Delete/{userId}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _mediator.Send(new DeleteUserCommand() { Id = userId });
            await GuardarAuditoria("Eliminar", anterior: new { Id = userId }, nuevo: null);
            return Ok(result);
        }

        [HttpGet("GetUserDetails/{userId}")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            try
            {
                var result = await _mediator.Send(new GetUserDetailsQuery() { UserId = userId });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserDetailsByUserName/{userName}")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetailsByUserName(string userName)
        {
            var result = await _mediator.Send(new GetUserDetailsByUserNameQuery() { UserName = userName });
            return Ok(result);
        }

        [HttpPost("AssignRoles")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> AssignRoles(AssignUsersRoleCommand command)
        {
            var result = await _mediator.Send(command);
            await GuardarAuditoria("AsignarRoles", anterior: null, nuevo: command);
            return Ok(result);
        }

        [HttpPut("EditUserRoles")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> EditUserRoles(UpdateUserRolesCommand command)
        {
            var result = await _mediator.Send(command);
            await GuardarAuditoria("EditarRoles", anterior: null, nuevo: command);
            return Ok(result);
        }

        [HttpGet("GetAllUserDetails")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetAllUserDetails()
        {
            var result = await _mediator.Send(new GetAllUsersDetailsQuery());
            return Ok(result);
        }


        [HttpPut("EditUserProfile/{id}")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> EditUserProfile(string id, [FromBody] EditUserProfileCommand command)
        {
            if (id == command.Id)
            {
                var result = await _mediator.Send(command);
                await GuardarAuditoria("EditarPerfil", anterior: null, nuevo: command);
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("GetUserRole/{userId}")]
        public IActionResult GetUserRole(string userId)
        {
            var result = _usurioRepo.UltimoRolPorIdUsuario(userId);
            return Ok(result);
        }

        [HttpGet("Auditoria")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> Auditoria(CancellationToken cancellationToken)
        {
            var historico = await _historico.GetHistoricoByTablaAsync(NombreTablaAudit, cancellationToken);
            return Ok(historico);
        }

        private async Task GuardarAuditoria(string transaccion, object? anterior, object? nuevo)
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
                await _historico.GuardarHistoricoAsync(historico, cancellationToken: default);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WARN audit Usuarios {transaccion} fallo: {ex.Message}");
            }
        }
    }
}
