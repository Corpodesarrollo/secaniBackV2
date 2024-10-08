using Core.DTOs;
using Core.Interfaces.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Web.MVCCore;

namespace MSAuthentication.Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class AuthController(IAuthRepo repo) : Controller
    {
        [HttpGet]
        public async Task<ActionResult<UserDto>> Get()
        {
            var user = this.GetUser();
            var result = await repo.GetUser(user);

            if (result == null)
                return BadRequest();

            return Ok(result);
        }
    }
}
