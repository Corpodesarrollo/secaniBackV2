using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using SISPRO.TRV.Entity;
using SISPRO.TRV.Entity.Helpers;

namespace Infra.Middleware
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        public User CurrentUser { get; }

        public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            CurrentUser = principal?.GetUser() ?? new User();
        }
    }
}
