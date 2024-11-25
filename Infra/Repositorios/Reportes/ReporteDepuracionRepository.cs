using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteDepuracionRepository(ApplicationDbContext context): IReporteDepuracionRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<ReporteDepuracion>> GetReporteDepuraciones(DateTime FechaInicio, DateTime FechaFin)
        {
            // Validación de las fechas
            if (FechaInicio > FechaFin)
                throw new ArgumentException("La fecha de inicio debe ser menor o igual a la fecha de fin.");

            var fechaInicioDateOnly = DateOnly.FromDateTime(FechaInicio);
            var fechaFinDateOnly = DateOnly.FromDateTime(FechaFin);

            // Filtro de datos
            var reportesFiltrados = await _context.ReporteDepuracions
                .Where(r => r.Fecha >= fechaInicioDateOnly && r.Fecha <= fechaFinDateOnly)
                .ToListAsync();

            return reportesFiltrados;
        }
    }
}
