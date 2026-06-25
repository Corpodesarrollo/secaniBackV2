using Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace MSAuthentication.Api.Controllers
{
    public class DiagnosticoController : BaseController
    {
        [HttpGet("Whoami")]
        public IActionResult Whoami()
        {
            var user = HttpContext.User;
            var result = new
            {
                IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
                AuthenticationType = user?.Identity?.AuthenticationType,
                Name = user?.Identity?.Name,
                ClaimsCount = user?.Claims?.Count() ?? 0,
                Claims = user?.Claims?.Select(c => new { Type = c.Type, Value = c.Value, Issuer = c.Issuer }).ToList(),
                RequestHeaders = HttpContext.Request.Headers
                    .Where(h => !h.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                CookieNames = HttpContext.Request.Cookies.Keys.ToList(),
                RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            return Ok(result);
        }
    }
}
