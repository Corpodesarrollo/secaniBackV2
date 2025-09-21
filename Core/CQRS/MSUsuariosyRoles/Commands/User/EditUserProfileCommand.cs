using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;

namespace Core.CQRS.MSUsuariosyRoles.Commands.User
{
    public class EditUserProfileCommand : IRequest<int>
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Telefonos { get; set; } = string.Empty;
        public string? EntidadId { get; set; } = string.Empty;
        public string? Cargo { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class EditUserProfileCommandHandler : IRequestHandler<EditUserProfileCommand, int>
    {
        private readonly IIdentityService _identityService;
        private readonly ISeguimientoRepo _seguimientoRepo;

        public EditUserProfileCommandHandler(IIdentityService identityService, ISeguimientoRepo seguimientoRepo)
        {
            _seguimientoRepo = seguimientoRepo;
            _identityService = identityService;
        }

        public async Task<int> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            if (request.Estado != true)
            {
                var reasignados = await _seguimientoRepo.AsignacionAutomaticaReasignacion();
            }

            var result = await _identityService.UpdateUserProfile(request.Id, request.FullName, request.Email, request.Telefonos, request.EntidadId, request.Cargo, request.Estado);
            return result ? 1 : 0;
        }
    }
}
