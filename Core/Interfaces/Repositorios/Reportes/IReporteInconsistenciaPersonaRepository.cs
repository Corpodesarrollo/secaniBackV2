using Core.Modelos;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteInconsistenciaPersonaRepository
    {
        Task<ReporteInconsistenciaPersona> GetReporteInconsistenciaAsync(long NNAId);
        Task<List<ReporteInconsistenciaPersona>> GetReporteInconsistenciasAsync();
        Task<ReporteInconsistenciaPersona> AddReporteInconsistenciaAsync(ReporteInconsistenciaPersona reporte);
    }
}
