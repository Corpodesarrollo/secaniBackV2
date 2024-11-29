using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoSeguimientoService(IReporteDinamicoSeguimientoRepository repository) : IReporteDinamicoSeguimientoService
    {
        private readonly IReporteDinamicoSeguimientoRepository _repository = repository;
        public Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken)
        {
            return _repository.GetReporteDinamicoSeguimientoAsync(FechaInicio, FechaFin, cancellationToken);
        }
    }
}
