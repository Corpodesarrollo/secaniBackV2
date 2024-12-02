using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class EnviarRespuestaController(IEnviarRespuesta repo) : Controller
    {
        [HttpPost]
        public Task<bool> Post(EnviarRespuestaDto data)
        {
            return repo.EnviarRespuesta(data);
        }
    }
}
