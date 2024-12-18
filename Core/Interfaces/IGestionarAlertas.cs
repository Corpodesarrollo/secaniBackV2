using Core.DTOs;

namespace Core.Interfaces
{
    public interface IGestionarAlertas
    {
        Task<RespuestasAlertaDto> Alerta(int idAlerta);
        Task<NotificacionEntidadDto> GetNotificacionEntidad(int idAlerta);
        List<GestionarAlertasDto> ObtenerAlertas(string alias);
    }
}
