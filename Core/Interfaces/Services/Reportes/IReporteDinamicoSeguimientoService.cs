using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDinamicoSeguimientoService
    {
        Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
    }
}
