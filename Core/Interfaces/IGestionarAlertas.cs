using Core.DTOs;
using Core.Response;

namespace Core.Interfaces
{
    public interface IGestionarAlertas
    {
        Task<RespuestasAlertaDto> Alerta(int idAlerta);
        Task<RespuestaResponse<bool>> EnviarRespuesta(EnviarRespuestaDto dto);
        Task<NotificacionEntidadDto> GetNotificacionEntidad(int idAlerta);
        List<GestionarAlertasDto> ObtenerAlertas(string alias);
    }
}
