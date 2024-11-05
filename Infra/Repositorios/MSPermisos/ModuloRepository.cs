using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSPermisos
{
    public class ModuloRepository(ApplicationDbContext context) : GenericRepository<TPModuloComponenteObjeto>(context), IModuloRepository
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<IList<TPModuloComponenteObjeto>> GetAllByIdPadreAsync(int idPadre, CancellationToken cancellationToken)
        {
            return await _context.TPModuloComponenteObjeto
                .Where(x => x.ModuloComponenteObjetoIdPadre == idPadre).ToListAsync();
        }

        async Task<IList<ModuloResponseDTO>> IModuloRepository.GetAllByIdPadreAsync(int idPadre, CancellationToken cancellationToken)
        {
            var items = await _context.TPModuloComponenteObjeto
                .Where(x => x.ModuloComponenteObjetoIdPadre == idPadre).ToListAsync();
            return items.Adapt<IList<ModuloResponseDTO>>();
        }
    }
}
