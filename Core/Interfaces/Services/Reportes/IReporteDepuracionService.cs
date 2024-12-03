using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDepuracionService
    {
        Task<List<ReporteDepuracionDTO>> GetReporteDepuraciones(DateTime FechaInicio, DateTime FechaFin);
    }
}
