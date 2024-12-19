using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoAlertasService(IReporteDinamicoAlertasRepository repository) : IReporteDinamicoAlertasService
    {
        private readonly IReporteDinamicoAlertasRepository _repository = repository;
        public async Task<List<ReporteDinamicoAlertasDTO>> GetReporteDinamicoAlertasAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken)
        {
            return await _repository.GetReporteDinamicoAlertasAsync(FechaInicio, FechaFin, cancellationToken);
        }
    }
}
