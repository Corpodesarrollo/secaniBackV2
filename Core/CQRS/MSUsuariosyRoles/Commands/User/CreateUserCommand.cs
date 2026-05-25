using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;
using System.Data;

namespace Core.CQRS.MSUsuariosyRoles.Commands.User
{
    // Cambia la interfaz implementada por CreateUserCommand de IRequest<int> a IRequest<string>
    public class CreateUserCommand : IRequest<string>
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Identificacion { get; set; }
        public string ConfirmarIdentificacion { get; set; }
        public string Telefonos { get; set; } = "";
        public string? EntidadId { get; set; }
        public string Cargo { get; set; }
        public string Alias { get; set; }
        public List<string> Roles { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly IIdentityService _identityService;
        public CreateUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.CreateUserAsync(request.Email, request.Identificacion, request.Email, request.FullName, request.Roles, request.Alias, request.Telefonos, request.EntidadId, request.Cargo);
            return result.isSucceed? result.userId : "";
        }
    }
}
