using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoEAPBService(IReporteDinamicoEAPBRepository repository) : IReporteDinamicoEAPBService
    {
        private readonly IReporteDinamicoEAPBRepository _repository = repository;
        public async Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            return await _repository.GetReporteDinamicoEAPBAsync(fechaInicio, fechaFin, cancellationToken);
        }
    }
}
