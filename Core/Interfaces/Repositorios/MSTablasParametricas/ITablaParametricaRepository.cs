using Core.Modelos.TablasParametricas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositorios.MSTablasParametricas
{
    public interface ITablaParametricaRepository
    {
        Task<IEnumerable<TablaParametrica>> GetAllAsync();
        Task<TablaParametrica> GetByIdAsync(string id);
        Task<TablaParametrica> AddAsync(TablaParametrica tabla);
        Task UpdateAsync(TablaParametrica tabla);
        Task RemoveAsync(string id);
        Task<IEnumerable<TablaParametrica>> GetTablasByPadreAsync(string padreId);
        Task<IEnumerable<TablaParametrica>> GetTablasByFuenteAsync(int fuenteId);
    }
}
