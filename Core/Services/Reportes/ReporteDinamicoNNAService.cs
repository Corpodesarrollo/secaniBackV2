using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoNNAService(IReporteDinamicoNNARepository repository) : IReporteDinamicoNNAService
    {
        private readonly IReporteDinamicoNNARepository _repository = repository;

        public async Task<List<NNAReporteDTO>> GetNNAForReporteAsync(CancellationToken cancellationToken)
        {
            return await _repository.GetNNAForReporteAsync(cancellationToken);
        }

        public async Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            return await _repository.GetReporteDinamicoNNAAsync(fechaInicio, fechaFin, cancellationToken);
        }
    }
}
