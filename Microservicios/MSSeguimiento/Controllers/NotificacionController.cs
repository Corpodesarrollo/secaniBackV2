using Core.Interfaces.Repositorios;
using Core.request;
using Core.Request;
using Core.response;
using Infra.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionRepo notificacionRepo;

        public NotificacionController(INotificacionRepo notificacion)
        {
            notificacionRepo = notificacion;
        }

        [HttpGet("GetNotification/{agenteDestinoId}")]
        public List<GetNotificacionResponse> GetNotifications(string agenteDestinoId)
        {
            List<GetNotificacionResponse> response;

            response = notificacionRepo.GetNotificacionUsuario(agenteDestinoId);

            return response;
        }

        [HttpGet("GetNumeroNotification/{AgenteDestinoId}")]
        public int GetNumeroNotifications(string agenteDestinoId)
        {
            return notificacionRepo.GetNumeroNotificacionUsuario(agenteDestinoId);
        }

        [HttpPost("OficioNotificacion")]
        public string GenerarOficioNotificacion(OficioNotificacionRequest request)
        {
            return notificacionRepo.GenerarOficioNotificacion(request);
        }

        [HttpPost("EliminarNotificacion")]
        public void EliminarNotificacion(EliminarNotificacionRequest request)
        {
            notificacionRepo.EliminarNotificacion(request);
        }


        [HttpGet("GetNotificacionEntidadCasos")]
        public List<GetNotificacionesEntidadResponse> GetNotificacionEntidadCasos(int entidadId, int alertaSeguimientoId, int nnaId)
        {

            List<GetNotificacionesEntidadResponse> response = notificacionRepo.RepoNotificacionEntidadCasos(entidadId, alertaSeguimientoId, nnaId);
            return response;
        }

        [HttpGet("GetListaCasosNotificacion")]
        public List<GetListaCasosResponse> GetListaCasosNotificacion(int eapbId, int epsId)
        {

            List<GetListaCasosResponse> response = notificacionRepo.RepoListaCasosNotificacion(eapbId, epsId);
            return response;
        }
    }
}
