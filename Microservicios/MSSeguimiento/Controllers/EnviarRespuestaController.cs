using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class EnviarRespuestaController(IEnviarRespuesta repo) : BaseController
    {
        [HttpPost]
        public Task<bool> Post(EnviarRespuestaDto data)
        {
            return repo.EnviarRespuesta(data);
        }
    }
}
