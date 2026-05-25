using Core.DTOs;
using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDinamicoSeguimientoService
    {
        Task<List<ReporteCasosEAPBDto>> GetReporteCasosEAPBAsync(ReporteCasosEAPBRequestDto request);
        Task<string> GetReporteCasosEAPBExcelAsync(ReporteCasosEAPBRequestDto request);
        Task<List<ReporteCasosEAPBDto>> GetReporteCasosEntidadAsync(ReporteCasosEntidadRequestDto request);
        Task<string> GetReporteCasosEntidadExcelAsync(ReporteCasosEntidadRequestDto request);
        Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken);
    }
}
