using Core.DTOs;

namespace Core.Interfaces
{
    public interface IEnviarRespuesta
    {
        Task<bool> EnviarRespuesta(EnviarRespuestaDto data);
    }
}
