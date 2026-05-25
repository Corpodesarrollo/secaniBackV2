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
            return MarcarKindUtc(response.Adapt<IEnumerable<HistoricoTransaccionDTO>>());
        }

        public async Task<IEnumerable<HistoricoTransaccionDTO>> GetHistoricosAsync(CancellationToken cancellationToken)
        {
            var response = await _repository.GetHistoricosAsync(cancellationToken);
            return MarcarKindUtc(response.Adapt<IEnumerable<HistoricoTransaccionDTO>>());
        }

        // BUG-LZ-027 timezone: SQL Server EC2 esta en UTC y backend guarda DateTime.UtcNow,
        // pero EF lee con Kind=Unspecified. System.Text.Json serializa SIN sufijo 'Z' y el
        // frontend Angular interpreta el string como local -> muestra hora UTC literal (+5h
        // en Colombia). Forzar Kind=Utc para que el JSON incluya 'Z' y Angular convierta a
        // la zona del navegador.
        private static IEnumerable<HistoricoTransaccionDTO> MarcarKindUtc(IEnumerable<HistoricoTransaccionDTO> items)
        {
            var lista = items.ToList();
            foreach (var item in lista)
            {
                if (item.FechaTransaccion.Kind != DateTimeKind.Utc)
                {
                    item.FechaTransaccion = DateTime.SpecifyKind(item.FechaTransaccion, DateTimeKind.Utc);
                }
            }
            return lista;
        }

        public async Task<bool> GuardarHistoricoAsync(HistoricoTransaccion historico, CancellationToken cancellationToken)
        {
            return await _repository.GuardarHistoricoAsync(historico, cancellationToken);
        }
    }
}
