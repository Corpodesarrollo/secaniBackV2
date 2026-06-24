using Core.DTOs.Reportes;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDinamicoEAPBRepository
    {
        Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime fechaInicio, DateTime fechaFin, int? eapbId, string? departamentoId, CancellationToken cancellationToken);
    }
}
