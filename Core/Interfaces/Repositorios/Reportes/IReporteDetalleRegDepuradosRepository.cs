using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDetalleRegDepuradosRepository
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(int IdReporteDepuracion, int TipoRegistro, CancellationToken cancellationToken);
    }
}
