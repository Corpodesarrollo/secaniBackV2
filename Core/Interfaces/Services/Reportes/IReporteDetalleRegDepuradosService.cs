using Core.DTOs.Reportes;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDetalleRegDepuradosService
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(int IdReporteDepuracion, int TipoRegistro, CancellationToken cancellationToken);
    }
}
