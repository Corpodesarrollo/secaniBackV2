using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Infra.Repositorios.MSTablasParametricas
{
    public class HistoricoTransaccionRepository(ApplicationDbContext context): IHistoricoTransaccionRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<HistoricoTransaccion>> GetHistoricoByTablaAsync(string nombreTabla, CancellationToken cancellationTokenationToken = default)
        {
            return await _context.Set<HistoricoTransaccion>()
            .Where(h => h.NombreTabla.ToUpper() == nombreTabla.ToUpper().Trim())
            .OrderByDescending(h => h.FechaTransaccion)
            .ToListAsync();
        }

        public async Task<IEnumerable<HistoricoTransaccion>> GetHistoricosAsync(CancellationToken cancellationToken)
        {
            return await _context.Set<HistoricoTransaccion>().ToListAsync(cancellationToken);
        }

        public async Task<bool> GuardarHistoricoAsync(HistoricoTransaccion historico, CancellationToken cancellationToken)
        {
            try
            {
                await _context.Set<HistoricoTransaccion>().AddAsync(historico, cancellationToken);
                var cambios = await _context.SaveChangesAsync(cancellationToken);
                return cambios > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
