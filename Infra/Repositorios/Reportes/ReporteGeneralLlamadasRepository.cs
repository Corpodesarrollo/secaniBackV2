using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteGeneralLlamadasRepository(ApplicationDbContext context) : IReporteGeneralLlamadasRepository
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<ReporteGeneralLlamadasDTO>> GetReporteGeneralLlamadasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            var resultados = await _context.Intentos
                .Where(i => i.FechaIntento.Date >= fechaInicio.Date && i.FechaIntento.Date <= fechaFin.Date)
                .GroupBy(i => new { i.CreatedByUserId, Fecha = i.FechaIntento.Date })
                .Select(g => new ReporteGeneralLlamadasDTO
                {
                    AgenteDeSeguimientoId = g.Key.CreatedByUserId,
                    AgenteDeSeguimiento = g.Key.CreatedByUserId,
                    FechaIntento = g.Key.Fecha, // Solo el día, sin la hora
                    LlamadasRealizadas = g.Count(),
                    Exitosas = g.Count(i => i.TipoFallaIntentoId == 0),
                    Fallidas = g.Count(i => i.TipoFallaIntentoId != 0),
                    NoContestan = g.Count(i => i.TipoFallaIntentoId == 1),
                    TelErrado = g.Count(i => i.TipoFallaIntentoId == 2),
                    TelOcupado = g.Count(i => i.TipoFallaIntentoId == 5),
                    BuzonDeVoz = g.Count(i => i.TipoFallaIntentoId == 6),
                    SinSenal = g.Count(i => i.TipoFallaIntentoId == 7),
                    Otro = g.Count(i => i.TipoFallaIntentoId == 8)
                })
                .OrderBy(r => r.AgenteDeSeguimiento)
                .ThenBy(r => r.FechaIntento)
                .ToListAsync();
            return resultados;
        }
    }
}
