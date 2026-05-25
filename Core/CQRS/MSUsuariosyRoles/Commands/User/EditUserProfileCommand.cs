using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;

namespace Core.CQRS.MSUsuariosyRoles.Commands.User
{
    public class EditUserProfileCommand : IRequest<EditUserProfileResult>
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Telefonos { get; set; } = string.Empty;
        public string? EntidadId { get; set; } = string.Empty;
        public string? Cargo { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class EditUserProfileResult
    {
        public int Code { get; set; }
        public string? Message { get; set; }
    }

    public class EditUserProfileCommandHandler : IRequestHandler<EditUserProfileCommand, EditUserProfileResult>
    {
        private readonly IIdentityService _identityService;
        private readonly ISeguimientoRepo _seguimientoRepo;

        public EditUserProfileCommandHandler(IIdentityService identityService, ISeguimientoRepo seguimientoRepo)
        {
            _seguimientoRepo = seguimientoRepo;
            _identityService = identityService;
        }

        public async Task<EditUserProfileResult> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            // BUG-015: bloquear inactivacion si es unico agente activo
            if (request.Estado != true)
            {
                var esUnicoAgente = await _identityService.IsUnicoAgenteActivo(request.Id!);
                if (esUnicoAgente)
                {
                    return new EditUserProfileResult
                    {
                        Code = -1,
                        Message = "No es posible inactivar el usuario debido a se el único agente de seguimiento en SECANI, solicitar autorización"
                    };
                }
                var reasignados = await _seguimientoRepo.AsignacionAutomaticaReasignacion();
            }

            var result = await _identityService.UpdateUserProfile(request.Id, request.FullName, request.Email, request.Telefonos, request.EntidadId, request.Cargo, request.Estado);
            return new EditUserProfileResult { Code = result ? 1 : 0 };
        }
    }
}
