using Core.Authorization;
using Core.Common;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class NotificacionController : BaseController
    {
        private readonly INotificacionRepo notificacionRepo;



        public NotificacionController(INotificacionRepo notificacion)
        {
            notificacionRepo = notificacion;
        }

        [HttpGet("GetNotification/{agenteDestinoId}")]
        public async Task<List<GetNotificacionResponse>> GetNotifications(string agenteDestinoId)
        {
            var response = await notificacionRepo.GetNotificacionUsuario(agenteDestinoId);

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
        public async Task<int> GetNumeroNotificationsAsync(string agenteDestinoId)
        {
            return await notificacionRepo.GetNumeroNotificacionUsuario(agenteDestinoId);
        }

        [HttpPost("SetNotification")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> SetNotification(GetNotificacionResponse request)
        {
            var result = await notificacionRepo.SetNotificacion(request);
            return Ok(result);
        }

        [HttpGet("ValidarNotificacion/{id}")]
        public async Task<IActionResult> ValidarNotificacion(int id)
        {
            var result = await notificacionRepo.ValidarNotificacion(id);
            return Ok(result);
        }

        [HttpPost("OficioNotificacion")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> GenerarOficioNotificacion(OficioNotificacionRequest request)
        {
            var result = await notificacionRepo.GenerarOficioNotificacion(request);
            return Ok(result);
        }

        [HttpPost("EliminarNotificacion")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public void EliminarNotificacion(EliminarNotificacionRequest request)
        {
            notificacionRepo.EliminarNotificacion(request);
        }

        [HttpPost("EnviarOficioNotificacion")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request)
        {
            var result = await notificacionRepo.EnviarOficioNotificacion(request);
            return Ok(result);
        }

        [HttpGet("VerOficioNotificacion/{id}")]
        public async Task<IActionResult> VerOficioNotificacion(long id)
        {
            var result = await notificacionRepo.VerOficioNotificacion(id);
            return Ok(result);
        }

        [HttpPost("NotificacionRespuesta")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> NotificacionRespuesta([FromForm] NotificacionRespuestaDto data)
        {
            var result = await notificacionRepo.NotificacionRespuesta(data);
            return Ok(result);
        }


        [HttpGet("GetNotificacionEntidadCasos")]
        public List<GetNotificacionesEntidadResponse> GetNotificacionEntidadCasos(long entidadId, int alertaSeguimientoId, int nnaId)
        {

            List<GetNotificacionesEntidadResponse> response = notificacionRepo.RepoNotificacionEntidadCasos(entidadId, alertaSeguimientoId, nnaId);
            return response;
        }

        [HttpGet("GetListaCasosNotificacion")]
        public List<GetListaCasosResponse> GetListaCasosNotificacion(string eapbId, int epsId)
        {

            List<GetListaCasosResponse> response = notificacionRepo.RepoListaCasosNotificacion(eapbId, epsId);
            return response;
        }


        [HttpPost("EnviarCorreo")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> EnviarCorreo([FromBody] CorreoRequest correoRequest)
        {
            if (correoRequest == null || string.IsNullOrEmpty(correoRequest.Body))
            {
                return BadRequest("El cuerpo del mensaje no puede estar vacío");
            }

            string resultado = await notificacionRepo.PlantillaCorreo(
                correoRequest.Para,
                correoRequest.ConCopia,
                correoRequest.Asunto,
                correoRequest.Body,
                correoRequest.Adjuntos,
                null
            );

            return Ok(new { mensaje = resultado });
        }

        [HttpPost("NotificacionReporteSivigila")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> NotificacionReporteSivigila([FromBody] NotificacionSigivilaRequest request)
        {

            Task<string> resultado = notificacionRepo.NotificacionReporteSivigila(request.idReporteSivigila, request.entidadId, request.userId);

            return Ok(new { mensaje = resultado });
        }

        [HttpPost("ProbarNotificacionReporteSivigila")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public async Task<IActionResult> ProbarEnvios()
        {
            await notificacionRepo.RevisarYEnviarNotificaciones();
            return Ok("Proceso ejecutado exitosamente");
        }


        [HttpPost("NotificacionSolicitudSeguimiento")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<IActionResult> NotificacionSolicitudSeguimiento([FromBody] NotificacionSolicitudSeguimientoRequest request)
        {

            Task<string> resultado = notificacionRepo.NotificacionSolicitudSeguimiento(request.cuidadorId!, request.nnaId, request.agenteSeguimientoId!, request.userId);


            return Ok(new { mensaje = resultado });
        }
    }
}

public class CorreoRequest
{
    public string[] Para { get; set; }
    public string[] ConCopia { get; set; }
    public string Asunto { get; set; }
    public string Body { get; set; }
    public string[] Adjuntos { get; set; }
}