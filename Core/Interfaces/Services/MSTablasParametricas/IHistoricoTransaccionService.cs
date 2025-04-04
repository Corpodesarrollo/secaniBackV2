using Core.DTOs.MSTablasParametricas;

namespace Core.Interfaces.Services.MSTablasParametricas
{
    public interface IHistoricoTransaccionService
    {
        Task<bool> GuardarHistoricoAsync(HistoricoTransaccion historico, CancellationToken cancellationToken);
        Task<IEnumerable<HistoricoTransaccionDTO>> GetHistoricosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<HistoricoTransaccionDTO>> GetHistoricoByTablaAsync(string nombreTabla, CancellationToken cancellationTokenationToken);
    }
}
