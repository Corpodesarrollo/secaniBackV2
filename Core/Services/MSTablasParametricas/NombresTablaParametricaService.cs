using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Mapster;

namespace Core.Services.MSTablasParametricas
{
    public class NombresTablaParametricaService(ITablaParametricaRepository repository) : INombreTablaParametricaService
    {
        private readonly ITablaParametricaRepository _repository = repository;
        public async Task<TablaParametricaDTO> AddAsync(TablaParametricaDTO tablaDTO)
        {
            var tabla = tablaDTO.Adapt<TablaParametrica>();
            var newTabla = await _repository.AddAsync(tabla);
            return newTabla.Adapt<TablaParametricaDTO>();
        }

        public async Task<IEnumerable<TablaParametricaDTO>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Adapt<IEnumerable<TablaParametricaDTO>>();
        }

        public async Task<TablaParametricaDTO> GetByIdAsync(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item.Adapt<TablaParametricaDTO>();
        }

        public async Task<IEnumerable<TablaParametricaDTO>> GetTablasByFuenteAsync(int idFuente)
        {
            var items = await _repository.GetTablasByFuenteAsync(idFuente);
            return items.Adapt<IEnumerable<TablaParametricaDTO>>();
        }

        public async Task<IEnumerable<TablaParametricaDTO>> GetTablasByPadreAsync(string idPadre)
        {
            var items = _repository.GetTablasByPadreAsync(idPadre);
            return items.Adapt<IEnumerable<TablaParametricaDTO>>();
        }

        public async Task RemoveAsync(string id)
        {
            await _repository.RemoveAsync(id);
        }

        public async Task UpdateAsync(TablaParametricaDTO tabla)
        {
            await _repository.UpdateAsync(tabla.Adapt<TablaParametrica>());
        }
    }
}
