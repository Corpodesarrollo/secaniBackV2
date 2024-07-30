using Core.DTOs.MSPermisos;

namespace Core.Services.MSPermisos
{
    public interface IModuloService
    {
        //Queries
        Task<ModuloResponseDTO> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<ModuloResponseDTO>> GetAllAsync(CancellationToken cancellationToken);

        //Commands
        Task<(bool, ModuloResponseDTO)> AddAsync(ModuloRequestDTO entity, CancellationToken cancellationToken);
        Task<(bool, ModuloResponseDTO)> UpdateAsync(ModuloResponseDTO entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(ModuloResponseDTO entity, CancellationToken cancellationToken);
    }
}
