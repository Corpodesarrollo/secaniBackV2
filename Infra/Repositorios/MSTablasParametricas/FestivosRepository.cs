using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.MSTablasParametricas
{
    public class FestivosRepository(ApplicationDbContext context)
        : GenericRepository<TPFestivos>(context), IFestivosRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<(bool, TPFestivos?)> EsFestivoAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var festivo = await _context.TPFestivos
                .FirstOrDefaultAsync(x => x.Festivo.Year == date.Year && x.Festivo.Month == date.Month && x.Festivo.Day == date.Day, cancellationToken);
            return (festivo != null, festivo);
        }

        public async Task<TPFestivos?> GetFestivoByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos
                .FirstOrDefaultAsync(x => x.Festivo.Year == date.Year && x.Festivo.Month == date.Month && x.Festivo.Day == date.Day, cancellationToken);
        }

        public async Task<IEnumerable<TPFestivos>> GetFestivosByAnoAndMesAsync(int ano, int mes, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos
                .Where(x => x.Festivo.Year == ano && x.Festivo.Month == mes)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TPFestivos>> GetFestivosByAnoAsync(int ano, CancellationToken cancellationToken)
        {
            return await _context.TPFestivos
                .Where(x => x.Festivo.Year == ano)
                .ToListAsync(cancellationToken);
        }

        // ✅ Crear desde request
        public async Task<(bool, TPFestivos?)> AddAsync(CreateFestivoRequest request)
        {
            var entity = new TPFestivos
            {
                Id = request.Id,
                Festivo = request.Festivo.ToDateTime(TimeOnly.MinValue),
                HoraInicio = request.HoraInicio,
                HoraFin = request.HoraFin,
                Orden = request.Orden,
                Activo = true,
                IsDeleted = false,
                FechaCreacion = DateTime.UtcNow
            };

            await _context.TPFestivos.AddAsync(entity);
            var result = await _context.SaveChangesAsync();
            return (result > 0, entity);
        }

        // ✅ Actualizar desde request
        public async Task<(bool, TPFestivos?)> UpdateAsync(UpdateFestivoRequest request)
        {
            var existing = await _context.TPFestivos.FindAsync(request.Id);
            if (existing == null) return (false, null);

            existing.Festivo = request.Festivo.ToDateTime(TimeOnly.MinValue);
            existing.HoraInicio = request.HoraInicio;
            existing.HoraFin = request.HoraFin;
            existing.Orden = request.Orden;

            _context.Entry(existing).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return (result > 0, existing);
        }
    }
}
