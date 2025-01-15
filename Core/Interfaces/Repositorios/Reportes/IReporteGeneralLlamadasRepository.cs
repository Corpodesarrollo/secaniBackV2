using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteGeneralLlamadasRepository
    {
        Task<List<ReporteGeneralLlamadasDTO>> GetReporteGeneralLlamadasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken);
    }
}
