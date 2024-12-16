using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDetalleRegDepuradosRepository
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(int TipoRegistro, CancellationToken cancellationToken);
    }
}
