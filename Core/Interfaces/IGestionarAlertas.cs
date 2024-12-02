using Core.DTOs;

namespace Core.Interfaces
{
    public interface IGestionarAlertas
    {
        Task<NotificacionEntidadDto> GetNotificacionEntidad(int idAlerta);
        List<GestionarAlertasDto> ObtenerAlertas(string alias);
    }
}
