using Core.DTOs;
using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteInconsistenciaPersonaService
    {
        Task<List<ReporteInconsistenciaPersonaDTO>> GetReporteInconsistenciasAsync();
        Task<ReporteInconsistenciaPersonaDTO> GetReporteInconsistenciaAsync(long id);
        Task<ReporteInconsistenciaPersonaDTO> AddReporteInconsistenciaPersonaAsync(NNADto menor);
    }
}
