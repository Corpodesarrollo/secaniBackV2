using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Infra.Repositories.Common;

namespace Infra.Repositorios.MSPermisos
{
    public class ModuloRepository(ApplicationDbContext context) : GenericRepository<TPModuloComponenteObjeto>(context), IModuloRepository
    {
    }
}
