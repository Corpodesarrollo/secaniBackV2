using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Mapster;

namespace Core.Services.MSTablasParametricas
{
    public class HistoricoTransaccionService(IHistoricoTransaccionRepository repository) : IHistoricoTransaccionService
    {
        private readonly IHistoricoTransaccionRepository _repository = repository;
        public async Task<IEnumerable<HistoricoTransaccionDTO>> GetHistoricoByTablaAsync(string nombreTabla, CancellationToken cancellationTokenationToken)
        {
            var response = await _repository.GetHistoricoByTablaAsync(nombreTabla, cancellationTokenationToken);
            return response.Adapt<IEnumerable<HistoricoTransaccionDTO>>();
        }

        public async Task<IEnumerable<HistoricoTransaccionDTO>> GetHistoricosAsync(CancellationToken cancellationToken)
        {
            var response = await _repository.GetHistoricosAsync(cancellationToken);
            return response.Adapt<IEnumerable<HistoricoTransaccionDTO>>();
        }

        public async Task<bool> GuardarHistoricoAsync(HistoricoTransaccion historico, CancellationToken cancellationToken)
        {
            return await _repository.GuardarHistoricoAsync(historico, cancellationToken);
        }
    }
}
