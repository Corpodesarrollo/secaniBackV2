using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;
using System.Data;

namespace Core.CQRS.MSUsuariosyRoles.Commands.User
{
    public class CreateUserCommand : IRequest<int>
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Identificacion { get; set; }
        public string ConfirmarIdentificacion { get; set; }
        public string Telefonos { get; set; } = "";
        public string? EntidadId { get; set; }
        public string Cargo { get; set; }
        public List<string> Roles { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
    {
        private readonly IIdentityService _identityService;
        public CreateUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.CreateUserAsync(request.Email, request.Identificacion, request.Email, request.FullName, request.Roles, request.Telefonos, request.EntidadId, request.Cargo);
            return result.isSucceed ? 1 : 0;
        }
    }
}
