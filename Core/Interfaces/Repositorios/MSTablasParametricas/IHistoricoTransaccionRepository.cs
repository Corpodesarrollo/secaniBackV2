using Core.DTOs.MSTablasParametricas;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSTablasParametricas
{
    public interface IHistoricoTransaccionRepository
    {
        Task<bool> GuardarHistoricoAsync(HistoricoTransaccion historico, CancellationToken cancellationToken);
        Task<IEnumerable<HistoricoTransaccion>> GetHistoricosAsync(CancellationToken cancellationToken);
        Task<IEnumerable<HistoricoTransaccion>> GetHistoricoByTablaAsync(string nombreTabla, CancellationToken cancellationTokenationToken);

    }
}
