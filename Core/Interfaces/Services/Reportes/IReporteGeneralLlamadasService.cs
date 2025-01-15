using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteGeneralLlamadasService
    {
        Task<List<ReporteGeneralLlamadasDTO>> GetReporteGeneralLlamadasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
    }
}
