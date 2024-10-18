using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
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

        [HttpGet("GetNotificationAlerta/{alertaId}")]
        public List<NotificacionResponse> GetNotificationsAlerta(long alertaId)
        {
            List<NotificacionResponse> response;

            response = notificacionRepo.GetNotificacionAlerta(alertaId);

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

        [HttpPost("EnviarOficioNotificacion")]
        public async Task<string> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request)
        {
            return await notificacionRepo.EnviarOficioNotificacion(request);
        }

        [HttpPost("VerOficioNotificacion")]
        public VerOficioNotificacionResponse VerOficioNotificacion(VerOficioNotificacionRequest request)
        {
            return notificacionRepo.VerOficioNotificacion(request);
        }

        [HttpPost("NotificacionRespuesta")]
        public async Task<bool?> NotificacionRespuesta([FromForm] NotificacionRespuestaDto data)
        {
            return await notificacionRepo.NotificacionRespuesta(data);
        }


        [HttpGet("GetNotificacionEntidadCasos")]
        public List<GetNotificacionesEntidadResponse> GetNotificacionEntidadCasos(long entidadId, int alertaSeguimientoId, int nnaId)
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
