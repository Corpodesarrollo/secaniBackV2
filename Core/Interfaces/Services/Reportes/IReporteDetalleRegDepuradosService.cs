using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDetalleRegDepuradosService
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(DateTime fechaInicio, DateTime fechaFin, int TipoRegistro, CancellationToken cancellationToken);
    }
}
