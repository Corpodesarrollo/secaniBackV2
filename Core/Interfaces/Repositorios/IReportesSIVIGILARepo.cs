using Core.DTOs;
using Core.Modelos;
using Core.Request;
using SISPRO.TRV.Entity;

namespace Core.Interfaces.Repositorios
{
    public interface IReportesSIVIGILARepo
    {
        Task<(bool, ReportesSIVIGILA)> AddAsync(ReportesSIVIGILADto data, User user);
        Task<UploadFileRequest?> EvidenciaDiagnostico(long id);
        Task<UploadFileRequest?> EvidenciaParentesco(long id);
        Task<IEnumerable<ReportesSIVIGILADto>> GetAll(CancellationToken cancellationToken);
        Task<ReportesSIVIGILADto?> GetById(long id);
        Task<(bool, ReportesSIVIGILA)> UpdateAsync(ReportesSIVIGILADto data);
        Task<bool> CrearSeguimiento(SeguimientoDto data, User user);
        // BUG-LZ-040: variante que retorna motivo cuando falla (NNA no existe, sin agente, etc.)
        Task<(bool success, string? message)> CrearSeguimientoDetallado(SeguimientoDto data, User user);
    }
}
