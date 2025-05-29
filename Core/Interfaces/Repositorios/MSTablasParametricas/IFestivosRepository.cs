using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.MSTablasParametricas
{
    public interface IFestivosRepository : IGenericRepository<TPFestivos>
    {
        public Task<IEnumerable<TPFestivos>> GetFestivosByAnoAsync(int ano, CancellationToken cancellationToken);
        public Task<IEnumerable<TPFestivos>> GetFestivosByAnoAndMesAsync(int ano, int mes, CancellationToken cancellationToken);
        public Task<TPFestivos?> GetFestivoByDateAsync(DateOnly date, CancellationToken cancellationToken);
        public Task<(bool, TPFestivos?)> EsFestivoAsync(DateOnly date, CancellationToken cancellationToken);

        // ✅ Métodos personalizados para crear y actualizar con request
        Task<(bool, TPFestivos?)> AddAsync(CreateFestivoRequest request);
        Task<(bool, TPFestivos?)> UpdateAsync(UpdateFestivoRequest request);
    }
}
