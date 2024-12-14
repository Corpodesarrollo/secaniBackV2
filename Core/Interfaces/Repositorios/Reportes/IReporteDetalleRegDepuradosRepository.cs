using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDetalleRegDepuradosRepository
    {
        Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(DateTime fechaInicio, DateTime fechaFin, int TipoRegistro, CancellationToken cancellationToken);
    }
}
