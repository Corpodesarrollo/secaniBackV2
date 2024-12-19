using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDinamicoAlertasRepository
    {
        Task<List<ReporteDinamicoAlertasDTO>> GetReporteDinamicoAlertasAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken);
    }
}
