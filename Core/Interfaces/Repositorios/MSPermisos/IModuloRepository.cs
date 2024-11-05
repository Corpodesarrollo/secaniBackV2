using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSPermisos
{
    public interface IModuloRepository : IGenericRepository<TPModuloComponenteObjeto>
    {
        public Task<IList<ModuloResponseDTO>> GetAllByIdPadreAsync(int idPadre, CancellationToken cancellationToken);
    }
}
