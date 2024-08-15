using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSPermisos
{
    public class PermisoRepository : GenericRepository<Permisos>, IPermisoRepository
    {
        private readonly ApplicationDbContext _context;

        public PermisoRepository(ApplicationDbContext context): base(context) 
        {
            _context = context;
        }

        public async Task<IList<Permisos>> GetPermisosByRoleId(string RoleId, CancellationToken cancellationToken)
        {
            return await _context.TPermisos.Where(x => x.RoleId == RoleId).ToListAsync();
        }

        public async Task<IList<Permisos>> GetPermisos(CancellationToken cancellationToken)
        {
            var items = await _context.TPermisos.ToListAsync();
            return items;
        }

        public async Task<(Permisos, TPFuncionalidad, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById(long id, CancellationToken cancellationToken)
        {
            var permiso = await _context.TPermisos.FirstOrDefaultAsync(x => x.Id == id);
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
