using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;

namespace Core.Interfaces.Services.MSTablasParametricas
{
    public interface ICategoriaAlertaService : IGenericService<TPCategoriaAlerta, CategoriaAlertaDTO>
    {
        public Task<CategoriaAlertaDTO> GetCategoriaAlertaWithSubCategorias(int id, CancellationToken cancellationToken);
    }
}
