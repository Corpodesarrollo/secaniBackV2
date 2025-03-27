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

        public async Task<IList<Permisos>> GetPermisosByModuloId(int ModuloId, CancellationToken cancellationToken)
        {
            var funcionalidades = await _context.TPModuloComponenteObjeto.Where(x => x.ModuloComponenteObjetoIdPadre == ModuloId).ToListAsync();
            var listaIds = funcionalidades.Select(m => m.Id).ToList();

            var permisosFiltrados = await _context.TPermisos
                                        .Where(p => listaIds.Contains(p.ModuloComponenteObjetoId ?? 0))
                                        .ToListAsync();
            return permisosFiltrados;
        }

        public async Task<IList<Permisos>> GetPermisosByRoleandModulo(string RoleId, int ModuloId, CancellationToken cancellationToken)
        {
            var funcionalidades = await _context.TPModuloComponenteObjeto.Where(x => x.ModuloComponenteObjetoIdPadre == ModuloId).ToListAsync();
            var listaIds = funcionalidades.Select(m => m.Id).ToList();

            var permisosFiltrados = await _context.TPermisos
                                        .Where(p => p.RoleId == RoleId && listaIds.Contains(p.ModuloComponenteObjetoId ?? 0))
                                        .ToListAsync();
            foreach (var item in funcionalidades)
            {
                var permiso = await _context.TPermisos.FirstOrDefaultAsync(x => x.RoleId == RoleId && x.ModuloComponenteObjetoId == item.Id);
                if (permiso == null)
                {
                    var permisoNuevo = new Permisos();
                    permisoNuevo.ModuloComponenteObjetoId = item.Id;
                    permisoNuevo.FuncionalidadId = ModuloId;
                    permisosFiltrados.Add(permisoNuevo);
                }
            }

            return permisosFiltrados;
        }

        public async Task<IList<Permisos>> GetPermisos(CancellationToken cancellationToken)
        {
            var items = await _context.TPermisos.ToListAsync();
            return items;
        }

        public async Task<(Permisos, TPModuloComponenteObjeto, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById(long id, CancellationToken cancellationToken)
        {
            var permiso = await _context.TPermisos.FirstOrDefaultAsync(x => x.Id == id);
            if (permiso == null)
            {
                return (null, null, null);
            }
            var funcionalidad = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == permiso.ModuloComponenteObjetoId);
            var modulo = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == funcionalidad.ModuloComponenteObjetoIdPadre);
            
            return (permiso, modulo, funcionalidad);
        }

        public async Task<(Permisos, TPModuloComponenteObjeto, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById0(Permisos permiso, CancellationToken cancellationToken)
        {
            if (permiso.Id != 0)
            {
                return (null, null, null);
            }
            var funcionalidad = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == permiso.ModuloComponenteObjetoId);
            var modulo = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == funcionalidad.ModuloComponenteObjetoIdPadre);
            return (permiso, modulo, funcionalidad);
        }
    }
}
