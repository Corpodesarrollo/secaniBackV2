using Core.Authorization;
using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class EnviarRespuestaController(IEnviarRespuesta repo) : BaseController
    {
        [HttpPost]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public Task<bool> Post(EnviarRespuestaDto data)
        {
            return repo.EnviarRespuesta(data);
        }
    }
}
