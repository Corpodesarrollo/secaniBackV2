using Core.DTOs;
using Core.Modelos;

namespace Core.Interfaces.Repositorios
{
    public interface IReportesSIVIGILARepo
    {
        Task<(bool, ReportesSIVIGILA)> AddAsync(ReportesSIVIGILADto data);
        Task<IEnumerable<ReportesSIVIGILADto>> GetAll(CancellationToken cancellationToken);
        Task<ReportesSIVIGILADto?> GetById(long id);
        Task<(bool, ReportesSIVIGILA)> UpdateAsync(ReportesSIVIGILADto data);
    }
}
