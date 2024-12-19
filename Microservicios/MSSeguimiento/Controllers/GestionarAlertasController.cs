using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class GestionarAlertasController(IGestionarAlertas repo) : Controller
    {
        [HttpGet("ConsultarAlertas/{alias}")]
        public List<GestionarAlertasDto> ConsultarAlertas(string alias)
        {
            return repo.ObtenerAlertas(alias);
        }

        [HttpGet("NotificacionEntidad/{idAlerta}")]
        public Task<NotificacionEntidadDto> NotificacionEntidad(int idAlerta)
        {
            return repo.GetNotificacionEntidad(idAlerta);
        }
    }
}
