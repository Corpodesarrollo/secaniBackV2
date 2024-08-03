using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSPermisos
{
    public class PermisoRepository(ApplicationDbContext context) : GenericRepository<Permiso>(context), IPermisoRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IList<Permiso>> GetPermisosByRoleId(string RoleId, CancellationToken cancellationToken)
        {
            return await _context.Permisos.Where(x => x.RoleId == RoleId).ToListAsync();
        }

        public async Task<(Permiso, TPFuncionalidad, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById(long id, CancellationToken cancellationToken)
        {
            var permiso = await _context.Permisos.FirstOrDefaultAsync(x => x.Id == id);
            if (permiso == null)
            {
                return (null, null, null);
            }
            var funcionalidad = await _context.TPFuncionalidad.FirstOrDefaultAsync(x => x.Id == permiso.FuncionalidadId);
            var modulo = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == permiso.ModuloComponenteObjetoId);
            return (permiso, funcionalidad, modulo);
        }
    }
}
