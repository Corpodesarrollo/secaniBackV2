using Core.DTOs.MSTablasParametricas;
using Core.Modelos;

namespace Core.Interfaces.MSTablasParametricas
{
    public interface IFestivoService : IGenericService<TPFestivos, FestivoDTO>
    {
        public Task<IEnumerable<FestivoDTO>> GetFestivosByAnoAsync(int ano, CancellationToken cancellationToken);
        public Task<IEnumerable<FestivoDTO>> GetFestivosByAnoAndMesAsync(int ano, int mes, CancellationToken cancellationToken);
        public Task<FestivoDTO?> GetFestivoByDateAsync(DateOnly date, CancellationToken cancellationToken);
        public Task<(bool, FestivoDTO?)> EsFestivoAsync(DateOnly date, CancellationToken cancellationToken);
    }
}
