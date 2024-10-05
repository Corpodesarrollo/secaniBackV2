using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSTablasParametricas
{
    public class TablaParametricaRepository(ApplicationDbContext context) : ITablaParametricaRepository
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<TablaParametrica> AddAsync(TablaParametrica tabla)
        {

            try
            {
                await _context.TablasParametricas.AddAsync(tabla);
                var result = await _context.SaveChangesAsync();
                return tabla;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<TablaParametrica>> GetAllAsync()
        {
            var items = await _context.TablasParametricas.AsNoTracking().ToListAsync();
            return items ?? Enumerable.Empty<TablaParametrica>();
        }

        public async Task<TablaParametrica> GetByIdAsync(string id)
        {
            return await _context.TablasParametricas.FindAsync(id);
        }

        public async Task<IEnumerable<TablaParametrica>> GetTablasByFuenteAsync(int fuenteId)
        {
            return await _context.TablasParametricas.Where(x => (int)x.FuenteTabla == fuenteId).ToListAsync();
        }

        public async Task<IEnumerable<TablaParametrica>> GetTablasByPadreAsync(string padreId)
        {
            return await _context.TablasParametricas.Where(x => x.TablaPadre == padreId).ToListAsync();
        }

        public async Task RemoveAsync(string id)
        {
            var tabla = _context.TablasParametricas.Find(id);
            if (tabla != null)
            {
                _context.TablasParametricas.Remove(tabla);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(TablaParametrica tabla)
        {
            _context.TablasParametricas.Update(tabla);
            await _context.SaveChangesAsync();
        }
    }
}
