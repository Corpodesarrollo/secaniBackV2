using Core.DTOs;
using Core.DTOs.Reportes;
using Core.Modelos;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteInconsistenciaPersonaService
    {
        Task<ReporteInconsistenciaPersonaDTO> GetReporteInconsistenciaPersonaByIdAsync(long NNAId);
        Task<List<ReporteInconsistenciaPersonaDTO>> GetReporteInconsistenciasPersonaAsync();
        Task<ReporteInconsistenciaPersonaDTO> AddReporteInconsistenciaAsync(NNADto menor);
        Task<InconsistenciaReporte> GetReporteInconsistenciasAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
