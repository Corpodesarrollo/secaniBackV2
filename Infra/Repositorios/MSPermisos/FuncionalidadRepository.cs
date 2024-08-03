using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;

namespace Infra.Repositorios.MSPermisos
{
    public class FuncionalidadRepository(ApplicationDbContext context) : GenericRepository<TPFuncionalidad>(context), IFuncionalidadRepository
    {
    }
}
