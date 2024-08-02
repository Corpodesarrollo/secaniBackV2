using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Modelos;
using Mapster;

namespace Core.Services.MSTablasParametricas
{
    public class FestivoService : GenericService<TPFestivos, FestivoDTO>, IFestivoService
    {
        private readonly IFestivosRepository _repository;

        public FestivoService(IFestivosRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<(bool, FestivoDTO?)> EsFestivoAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var (success, response) = await _repository.EsFestivoAsync(date, cancellationToken);
            return (success, response.Adapt<FestivoDTO>());
        }

        public async Task<FestivoDTO?> GetFestivoByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            var response = await _repository.GetFestivoByDateAsync(date, cancellationToken);
            if (response == null)
            {
                return null;
            }
            return response.Adapt<FestivoDTO>();
        }

        public async Task<IEnumerable<FestivoDTO>> GetFestivosByAnoAndMesAsync(int ano, int mes, CancellationToken cancellationToken)
        {
            var response = await _repository.GetFestivosByAnoAndMesAsync(ano, mes, cancellationToken);
            return response.Adapt<IEnumerable<FestivoDTO>>();
        }

        public async Task<IEnumerable<FestivoDTO>> GetFestivosByAnoAsync(int ano, CancellationToken cancellationToken)
        {
            var response = await _repository.GetFestivosByAnoAsync(ano, cancellationToken);
            return response.Adapt<IEnumerable<FestivoDTO>>();
        }
    }
}
