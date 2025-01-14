using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteGeneralLlamadasService(IReporteGeneralLlamadasRepository repository): IReporteGeneralLlamadasService
    {
        private readonly IReporteGeneralLlamadasRepository _repository = repository;

        public async Task<List<ReporteGeneralLlamadasDTO>> GetReporteGeneralLlamadasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            return await _repository.GetReporteGeneralLlamadasAsync(fechaInicio, fechaFin, cancellationToken);
        }
    }
}
