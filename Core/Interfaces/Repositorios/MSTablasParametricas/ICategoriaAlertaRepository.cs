using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.Common;
using Core.Modelos;
using Core.Modelos.TablasParametricas;

namespace Core.Interfaces.Repositorios.MSTablasParametricas
{
    public interface ICategoriaAlertaRepository : IGenericRepository<TPCategoriaAlerta>
    {
        public Task<CategoriaAlertaDTO> GetCategoriaAlertaWithSubCategorias(int id, CancellationToken cancellationToken);
    }
}
