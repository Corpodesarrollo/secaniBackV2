using Core.DTOs.MSTablasParametricas;

namespace Core.Interfaces.Services.MSTablasParametricas
{
    public interface INombreTablaParametricaService
    {
        Task<IEnumerable<TablaParametricaDTO>> GetAllAsync();
        Task<TablaParametricaDTO> GetByIdAsync(string id);
        Task<TablaParametricaDTO> AddAsync(TablaParametricaDTO tabla);
        Task UpdateAsync(TablaParametricaDTO tabla);
        Task RemoveAsync(string id);
        Task<IEnumerable<TablaParametricaDTO>> GetTablasByPadreAsync(string idPadre);
        Task<IEnumerable<TablaParametricaDTO>> GetTablasByFuenteAsync(int idFuente);
    }
}
