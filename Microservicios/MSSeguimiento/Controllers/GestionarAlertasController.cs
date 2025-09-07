using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Core.Response;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class GestionarAlertasController(IGestionarAlertas repo) : BaseController
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

        [HttpGet("Alerta/{idAlerta}")]
        public async Task<RespuestasAlertaDto> Alerta(int idAlerta)
        {
            return await repo.Alerta(idAlerta);
        }

        [HttpPost("EnviarRespuesta")]
        public async Task<RespuestaResponse<bool>> EnviarRespuesta(EnviarRespuestaDto dto)
        {
            return await repo.EnviarRespuesta(dto);
        }
    }
}
