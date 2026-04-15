using Core.Common;
using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Web.MVCCore;

namespace MSNNA.Api.Controllers
{
    public class AuthController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<UserDto>> Get()
        {
            var user = this.GetUser();

            return Ok(user);
        }
    }
}
