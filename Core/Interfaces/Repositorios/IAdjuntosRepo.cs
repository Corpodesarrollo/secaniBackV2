using Core.DTOs;

namespace Core.Interfaces.Repositorios
{
    public interface IAdjuntosRepo
    {
        Task<long> AddAdjunto(AdjuntosDto adjunto);
        Task<AdjuntosDto?> GetById(int id);
    }
}
