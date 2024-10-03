using Core.DTOs.MSPermisos;
using Core.request;
using Core.response;

namespace Core.Interfaces.Repositorios
{
    public interface IPermisosRepo
    {
        public List<GetVwMenuResponse> MenuXRolId(GetVwMenuRequest request, CancellationToken cancellationToken);

        //Queries
        Task<PermisoResponseDTO> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllByRoleIdAsync(string RoleId, CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllByModuloIdAsync(int ModuloId, CancellationToken cancellationToken);
        Task<IEnumerable<PermisoResponseDTO>> GetAllByModuloandRoleAsync(string RoleId, int ModuloId, CancellationToken cancellationToken);

        //Commands
        Task<(bool, PermisoResponseDTO)> AddAsync(PermisoRequestDTO entity, CancellationToken cancellationToken);
        Task<(bool, PermisoResponseDTO)> UpdateAsync(PermisoResponseDTO entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(PermisoResponseDTO entity, CancellationToken cancellationToken);
        Task ClearCacheAsync(string cacheKey, string roleId, string id);
    }
}
