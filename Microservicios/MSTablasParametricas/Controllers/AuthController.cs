﻿using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Web.MVCCore;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class AuthController : Controller
    {
        [HttpGet]
        public async Task<ActionResult<UserDto>> Get()
        {
            var user = this.GetUser();
            return Ok(user);
        }
    }
}
