using Microsoft.AspNetCore.Authorization;

namespace Core.Authorization
{
    public class RequiereRolRequirement : IAuthorizationRequirement
    {
        public string[] RolesPermitidos { get; }

        public RequiereRolRequirement(params string[] rolesPermitidos)
        {
            RolesPermitidos = rolesPermitidos ?? Array.Empty<string>();
        }
    }
}
