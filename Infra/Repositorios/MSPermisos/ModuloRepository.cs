using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSPermisos
{
    public class ModuloRepository(ApplicationDbContext context) : GenericRepository<TPModuloComponenteObjeto>(context), IModuloRepository
    {
        public readonly ApplicationDbContext _context = context;
        public async Task<IList<TPModuloComponenteObjeto>> GetModulos(CancellationToken cancellationToken)
        {
            return await _context.TPModuloComponenteObjeto.Where(x => x.ModuloComponenteObjetoIdPadre == 0).ToListAsync();
        }

        public async Task<IList<TPModuloComponenteObjeto>> GetModulosByPadreId(int PadreId, CancellationToken cancellationToken)
        {
            return await _context.TPModuloComponenteObjeto.Where(x => x.ModuloComponenteObjetoIdPadre == PadreId).ToListAsync();
        }
    }
}
