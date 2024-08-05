using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSPermisos
{
    public interface IPermisoRepository : IGenericRepository<Permiso>
    {
        public Task<(Permiso, TPFuncionalidad, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById(long id, CancellationToken cancellationToken);
        public Task<IList<Permiso>> GetPermisosByRoleId(string RoleId, CancellationToken cancellationToken);
    }
}
