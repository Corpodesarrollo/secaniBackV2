﻿using Core.Interfaces.Services.MSUsuariosyRoles;
using MediatR;

namespace Core.CQRS.MSUsuariosyRoles.Commands.User
{
    public class EditUserProfileCommand : IRequest<int>
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; } = string.Empty;
        public string EntidadId { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string ActivoName { get; set; } = string.Empty;
    }

    public class EditUserProfileCommandHandler : IRequestHandler<EditUserProfileCommand, int>
    {
        private readonly IIdentityService _identityService;

        public EditUserProfileCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        public async Task<int> Handle(EditUserProfileCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.UpdateUserProfile(request.Id, request.FullName, request.Email, request.Telefonos, request.EntidadId, request.Cargo, request.ActivoName);
            return result ? 1 : 0;
        }
    }
}
