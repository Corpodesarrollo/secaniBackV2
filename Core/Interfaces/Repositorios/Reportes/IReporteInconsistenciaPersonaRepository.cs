using Core.DTOs.Reportes;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteInconsistenciaPersonaRepository
    {
        Task<ReporteInconsistenciaPersona> GetReporteInconsistenciaPersonaByIdAsync(long NNAId);
        Task<List<ReporteInconsistenciaPersona>> GetReporteInconsistenciasPersonaAsync();
        Task<ReporteInconsistenciaPersona> AddReporteInconsistenciaAsync(ReporteInconsistenciaPersona reporte);
        Task<InconsistenciaReporte> GetReporteInconsistenciasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<bool> MarcarResueltoAsync(long reporteId, string? userId);
        Task<int> ContarReportesPreviosNNAAsync(long nnaId);
    }
}
