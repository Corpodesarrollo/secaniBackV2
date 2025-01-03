using Core.DTOs;
using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDinamicoSeguimientoRepository
    {
        Task<List<ReporteCasosEAPBDto>> GetReporteCasosEAPBAsync(ReporteCasosEAPBRequestDto request);
        Task<string> GetReporteCasosEAPBExcelAsync(ReporteCasosEAPBRequestDto request);
        Task<List<ReporteCasosEAPBDto>> GetReporteCasosEntidadAsync(ReporteCasosEntidadRequestDto request);
        Task<string> GetReporteCasosEntidadExcelAsync(ReporteCasosEntidadRequestDto request);
        Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
    }
}
