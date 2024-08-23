using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using Core.Modelos.TablasParametricas;

namespace Core.Services.MSTablasParametricas
{

    
    public class CategoriaAlertaService : GenericService<TPCategoriaAlerta, CategoriaAlertaDTO>, ICategoriaAlertaService
    {
        private readonly ICategoriaAlertaRepository _repository;

        public CategoriaAlertaService(ICategoriaAlertaRepository repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task<CategoriaAlertaDTO> GetCategoriaAlertaWithSubCategorias(int id, CancellationToken cancellationToken)
        {
            return await _repository.GetCategoriaAlertaWithSubCategorias(id, cancellationToken);
        }
    }
}
