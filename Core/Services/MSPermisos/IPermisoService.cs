using Core.DTOs.MSPermisos;
using Core.Modelos;

namespace Core.Services.MSPermisos
{
    public interface IPermisoService
    {
        //Queries
        Task<PermisoResponseDTO> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllByRoleIdAsync(string RoleId, CancellationToken cancellationToken);

        //Commands
        Task<(bool, PermisoResponseDTO)> AddAsync(PermisoRequestDTO entity, CancellationToken cancellationToken);
        Task<(bool, PermisoResponseDTO)> UpdateAsync(PermisoResponseDTO entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(PermisoResponseDTO entity, CancellationToken cancellationToken);
        Task ClearCacheAsync(string cacheKey, string roleId, string id);
    }
}
