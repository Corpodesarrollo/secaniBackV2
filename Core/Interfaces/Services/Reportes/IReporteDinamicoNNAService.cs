using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDinamicoNNAService
    {
        Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
    }
}
