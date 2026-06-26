using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Authorization
{
    public class RequiereRolHandler : AuthorizationHandler<RequiereRolRequirement>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<RequiereRolHandler> _logger;

        public RequiereRolHandler(IConfiguration config, ILogger<RequiereRolHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequiereRolRequirement requirement)
        {
            bool enabled = _config.GetValue<bool>("Permisos:EnablePermissionPolicy", false);

            if (!enabled)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("RequiereRol: usuario no autenticado");
                return Task.CompletedTask;
            }

            var claimTypesRol = new[] { "role", "roles", "groups", "UserGroups", "RolCode", "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" };

            var rolesUsuario = context.User.Claims
                .Where(c => claimTypesRol.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToList();

            if (rolesUsuario.Any(r => requirement.RolesPermitidos.Contains(r, StringComparer.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("RequiereRol: usuario {Name} no tiene roles requeridos {Roles}. Roles actuales: {Actuales}",
                    context.User.Identity?.Name,
                    string.Join(",", requirement.RolesPermitidos),
                    string.Join(",", rolesUsuario));
            }

            return Task.CompletedTask;
        }
    }
}
