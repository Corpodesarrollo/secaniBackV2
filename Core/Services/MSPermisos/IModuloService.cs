using Core.DTOs.MSPermisos;
using Core.Modelos;

namespace Core.Services.MSPermisos
{
    public interface IModuloService
    {
        //Queries
        Task<ModuloResponseDTO> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<ModuloResponseDTO>> GetAllAsync(CancellationToken cancellationToken);
        Task<IList<ModuloResponseDTO>> GetModulos(CancellationToken cancellationToken);
        Task<IList<ModuloResponseDTO>> GetModulosByPadreId(int PadreId, CancellationToken cancellationToken);

        //Commands
        Task<(bool, ModuloResponseDTO)> AddAsync(ModuloRequestDTO entity, CancellationToken cancellationToken);
        Task<(bool, ModuloResponseDTO)> UpdateAsync(ModuloResponseDTO entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(ModuloResponseDTO entity, CancellationToken cancellationToken);
    }
}
