using Core.DTOs.MSUsuariosyRoles;
using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;

namespace Core.CQRS.MSUsuariosyRoles.Queries.User
{
    public class GetAllUsersByEntidadIdDetailsQuery : IRequest<List<UserDetailsResponseDTO>>
    {
        public string EntidadId { get; set; }
    }

    public class GetAllUsersByEntidadIdDetailsQueryHandler : IRequestHandler<GetAllUsersByEntidadIdDetailsQuery, List<UserDetailsResponseDTO>>
    {
        private readonly IIdentityService _identityService;

        public GetAllUsersByEntidadIdDetailsQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<List<UserDetailsResponseDTO>> Handle(GetAllUsersByEntidadIdDetailsQuery request, CancellationToken cancellationToken)
        {


            var users = await _identityService.GetAllUsersByEntidadIdAsync(request.EntidadId);
            var userDetails = users.Select(x => new UserDetailsResponseDTO()
            {
                Id = x.id,
                Email = x.email,
                UserName = x.userName,
                FullName = x.fullName,
                Telefonos = x.telefonos,
                EntidadId = x.entidadId,
                Cargo = x.cargo,
                Estado = x.Estado
            }).ToList();

            foreach (var user in userDetails)
            {
                user.Roles = await _identityService.GetUserRolesAsync(user.Id);
            }
            return userDetails;
        }
    }
}
