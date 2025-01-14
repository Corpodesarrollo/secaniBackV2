using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteInconsistenciaPersonaRepository : IReporteInconsistenciaPersonaRepository
    {
        private readonly ApplicationDbContext _context;

        public ReporteInconsistenciaPersonaRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ReporteInconsistenciaPersona> AddReporteInconsistenciaAsync(ReporteInconsistenciaPersona reporte)
        {
            try
            {
                if (reporte == null)
                    throw new ArgumentNullException(nameof(reporte), "El reporte no puede ser nulo.");

                _context.ReporteInconsistenciaPersona.Add(reporte);
                await _context.SaveChangesAsync();

                return reporte;
            }
            catch (DbUpdateException ex)
            {
                // Manejo de errores relacionados con la base de datos
                throw new InvalidOperationException("Error al agregar el reporte a la base de datos.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                throw new Exception("Ocurrió un error inesperado al agregar el reporte.", ex);
            }
        }

        public async Task<ReporteInconsistenciaPersona> GetReporteInconsistenciaAsync(long NNAId)
        {
            try
            {
                var reporte = await _context.ReporteInconsistenciaPersona.FirstOrDefaultAsync(r => r.NNAId == NNAId);

                if (reporte == null)
                    throw new KeyNotFoundException($"No se encontró ningún reporte con NNAId: {NNAId}.");

                return reporte;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurrió un error al obtener el reporte con NNAId: {NNAId}.", ex);
            }
        }

        public async Task<List<ReporteInconsistenciaPersona>> GetReporteInconsistenciasAsync()
        {
            try
            {
                return await _context.ReporteInconsistenciaPersona.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al obtener la lista de reportes de inconsistencias.", ex);
            }
        }
    }

}
