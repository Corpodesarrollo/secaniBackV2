using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSPermisos
{
    public interface IModuloRepository : IGenericRepository<TPModuloComponenteObjeto>
    {
        public Task<IList<TPModuloComponenteObjeto>> GetModulos(CancellationToken cancellationToken);
        public Task<IList<TPModuloComponenteObjeto>> GetModulosByPadreId(int PadreId, CancellationToken cancellationToken);
    }
}
