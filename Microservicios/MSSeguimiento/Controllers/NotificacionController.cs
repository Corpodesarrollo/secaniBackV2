using Core.Interfaces.Repositorios;
using Core.request;
using Core.Request;
using Core.response;
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

        [HttpPost("GetNotification")]
        public List<GetNotificacionResponse> GetNotifications(GetNotificacionRequest request)
        {
            List<GetNotificacionResponse> response;

            response = notificacionRepo.GetNotificacionUsuario(request.AgenteDestinoId);

            return response;
        }

        [HttpPost("GetNumeroNotification")]
        public int GetNumeroNotifications(GetNotificacionRequest request)
        {
            return notificacionRepo.GetNumeroNotificacionUsuario(request.AgenteDestinoId);
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
    }
}
