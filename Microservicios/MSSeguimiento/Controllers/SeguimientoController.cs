using Core.DTOs;
using Infra.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeguimientoController(SeguimientoRepo repos) : ControllerBase
    {
        [HttpGet("GetAllByIdUser/{IdUser}/{filtro}")]
        public ActionResult<List<SeguimientoDto>> GetAllByIdUser(string IdUser, int filtro)
        {
            var response = repos.GetAllByIdUser(IdUser, filtro);
            return response;
        }

        [HttpGet("GetCntSeguimiento/{IdUser}")]
        public ActionResult<SeguimientoCntFiltrosDto> GetCntSeguimiento(string IdUser)
        {
            var response = repos.SeguimientoCntFiltros(IdUser);
            return response;
        }
    }
}
