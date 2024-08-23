using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.MSTablasParametricas
{
    public class FestivosRepository(ApplicationDbContext context) : GenericRepository<TPFestivos>(context), IFestivosRepository
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<(bool, TPFestivos?)> EsFestivoAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var festivo = await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo.Year == date.Year && x.Festivo.Month == date.Month && x.Festivo.Day == date.Day);
            return (festivo != null, festivo);
        }

        public async Task<TPFestivos?> GetFestivoByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo.Year == date.Year && x.Festivo.Month == date.Month && x.Festivo.Day == date.Day, cancellationToken);
        }

        public async Task<IEnumerable<TPFestivos>> GetFestivosByAnoAndMesAsync(int ano, int mes, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos.Where(x => x.Festivo.Year == ano && x.Festivo.Month == mes).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TPFestivos>> GetFestivosByAnoAsync(int ano, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos.Where(x => x.Festivo.Year == ano).ToListAsync(cancellationToken);
        }
    }
}