using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infra.Repositorios.MSPermisos
{
    public class PermisoRepository : GenericRepository<Permisos>, IPermisoRepository
    {
        private readonly ApplicationDbContext _context;

        public PermisoRepository(ApplicationDbContext context) : base(context)
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
            var listaIds = funcionalidades == null ? new List<int>() : funcionalidades.Select(m => m.Id).ToList();

            var permisosFiltrados = await _context.TPermisos
                                        .Where(p => listaIds.Contains(p.ModuloComponenteObjetoId ?? 0))
                                        .ToListAsync();
            return permisosFiltrados;
        }

        public async Task<(Permisos?, TPModuloComponenteObjeto?)> CansByPathAndRoleId(
            string path,
            string roleId,
            CancellationToken cancellationToken)
        {
            var decoded = WebUtility.UrlDecode(path ?? string.Empty)?.Trim('/') ?? string.Empty;
            var parts = decoded.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // ===== CASO 1: SIN "/"  -> módulo raíz (Padre = 0) =====
            if (parts.Length == 1)
            {
                var raiz = parts[0];

                // Buscar módulo raíz por Path
                var moduloRaiz = await _context.TPModuloComponenteObjeto
                    .Where(m => m.ModuloComponenteObjetoIdPadre == 0
                                && m.Path != null
                                && m.Path.ToLower() == raiz.ToLower())
                    .FirstOrDefaultAsync(cancellationToken);

                if (moduloRaiz == null)
                    return (null, null);

                var permisoRes = await _context.TPermisos
                    .FirstOrDefaultAsync(p => p.ModuloComponenteObjetoId == moduloRaiz.Id
                                           && p.RoleId == roleId, cancellationToken);

                return (permisoRes, moduloRaiz);
            }

            // ===== CASO 2: CON "/" -> padre/hijo =====
            var padreNombre = parts[0];
            var hijoNombre = parts[1];

            // Padre por Path 
            var padre = await _context.TPModuloComponenteObjeto
                .Where(m => m.Path != null && m.Path.ToLower() == padreNombre.ToLower())
                .Select(m => new { m.Id })
                .FirstOrDefaultAsync(cancellationToken);

            if (padre == null) return (null, null);
            var padreId = padre.Id;

            // Hijo: Nombre contiene hijoNombre (CI) y Padre = padreId
            var hijo = await _context.TPModuloComponenteObjeto
                .Where(m =>
                    m.ModuloComponenteObjetoIdPadre == padreId &&
                    m.Nombre != null &&
                    EF.Functions.Like(m.Path.ToLower(), $"%{hijoNombre.ToLower()}%"))
                .FirstOrDefaultAsync(cancellationToken);

            if (hijo == null) return (null, null);

            var permisoFinal = await _context.TPermisos
                .FirstOrDefaultAsync(p => p.ModuloComponenteObjetoId == hijo.Id
                                       && p.RoleId == roleId, cancellationToken);

            var moduloFinal = hijo;
            return (permisoFinal, moduloFinal);
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
            var modulo = funcionalidad == null ? null : await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == funcionalidad.ModuloComponenteObjetoIdPadre);

            return (permiso, modulo, funcionalidad);
        }

        public async Task<(Permisos, TPModuloComponenteObjeto, TPModuloComponenteObjeto)> GetPermisoWithFuncionalidadAndModuloById0(Permisos permiso, CancellationToken cancellationToken)
        {
            if (permiso.Id != 0)
            {
                return (null, null, null);
            }
            var funcionalidad = await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == permiso.ModuloComponenteObjetoId);
            var modulo = funcionalidad == null ? null : await _context.TPModuloComponenteObjeto.FirstOrDefaultAsync(x => x.Id == funcionalidad.ModuloComponenteObjetoIdPadre);
            return (permiso, modulo, funcionalidad);
        }
    }
}
