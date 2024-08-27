using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSPermisos
{
    public interface IPermisoRepository : IGenericRepository<Permisos>
    {
        public Task<(Permisos, TPFuncionalidad, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById(long id, CancellationToken cancellationToken);
        public Task<IList<Permisos>> GetPermisos(CancellationToken cancellationToken);
        public Task<IList<Permisos>> GetPermisosByRoleId(string RoleId, CancellationToken cancellationToken);
        public Task<IList<Permisos>> GetPermisosByRoleandModulo(string RoleId, int ModuloId, CancellationToken cancellationToken);
        public Task<IList<Permisos>> GetPermisosByModuloId(int ModuloId, CancellationToken cancellationToken);
    }
}
