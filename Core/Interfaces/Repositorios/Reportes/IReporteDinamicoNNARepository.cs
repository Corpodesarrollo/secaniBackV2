using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDinamicoNNARepository
    {
        Task<List<NNAReporteDTO>> GetNNAForReporteAsync(CancellationToken cancellationToken);
        Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken);
    }
}
