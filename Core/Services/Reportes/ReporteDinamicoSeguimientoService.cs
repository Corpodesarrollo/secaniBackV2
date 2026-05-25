using Core.DTOs;
using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoSeguimientoService(IReporteDinamicoSeguimientoRepository repository) : IReporteDinamicoSeguimientoService
    {
        private readonly IReporteDinamicoSeguimientoRepository _repository = repository;

        public Task<List<ReporteCasosEAPBDto>> GetReporteCasosEAPBAsync(ReporteCasosEAPBRequestDto request)
        {
            return _repository.GetReporteCasosEAPBAsync(request);
        }

        public Task<string> GetReporteCasosEAPBExcelAsync(ReporteCasosEAPBRequestDto request)
        {
            return _repository.GetReporteCasosEAPBExcelAsync(request);
        }

        public Task<List<ReporteCasosEAPBDto>> GetReporteCasosEntidadAsync(ReporteCasosEntidadRequestDto request)
        {
            return _repository.GetReporteCasosEntidadAsync(request);
        }

        public Task<string> GetReporteCasosEntidadExcelAsync(ReporteCasosEntidadRequestDto request)
        {
            return _repository.GetReporteCasosEntidadExcelAsync(request);
        }

        public Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken)
        {
            return _repository.GetReporteDinamicoSeguimientoAsync(FechaInicio, FechaFin, cancellationToken);
        }

    }
}
