using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDetalleRegDepuradosService
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(int TipoRegistro, CancellationToken cancellationToken);
    }
}
