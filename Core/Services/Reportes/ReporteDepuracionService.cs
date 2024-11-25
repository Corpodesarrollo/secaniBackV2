using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;
using Mapster;

namespace Core.Services.Reportes
{
    public class ReporteDepuracionService(IReporteDepuracionRepository repository):IReporteDepuracionService
    {
        private readonly IReporteDepuracionRepository _repository = repository;

        public async Task<List<ReporteDepuracionDTO>> GetReporteDepuraciones(DateTime FechaInicio, DateTime FechaFin)
        {
            var result = await _repository.GetReporteDepuraciones(FechaInicio, FechaFin);
            return result.Adapt<List<ReporteDepuracionDTO>>();
        }
    }
}
