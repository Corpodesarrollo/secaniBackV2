using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDinamicoAlertasService
    {
        Task<List<ReporteDinamicoAlertasDTO>> GetReporteDinamicoAlertasAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken);
    }
}
