using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;

namespace Core.Services.Reportes
{
    public class ReporteDetalleRegDepuradosService(IReporteDetalleRegDepuradosRepository repository) : IReporteDetalleRegDepuradosService
    {
        private readonly IReporteDetalleRegDepuradosRepository _repository = repository;
        public async Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(DateTime fechaInicio, DateTime fechaFin, int TipoRegistro, CancellationToken cancellationToken)
        {
            return await _repository.GetReporteDetalleRegDepuradosAsync(fechaInicio, fechaFin, TipoRegistro, cancellationToken);
        }
    }
}
