using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDinamicoSeguimientoRepository
    {
        Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken);
    }
}
