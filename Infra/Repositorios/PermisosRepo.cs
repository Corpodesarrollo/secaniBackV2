using Core.Interfaces.Repositorios;
using Core.request;
using Core.response;

namespace Infra.Repositories
{
    public class PermisosRepo : IPermisosRepo
    {
        private readonly ApplicationDbContext _context;

        public PermisosRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<GetVwMenuResponse> MenuXRolId(GetVwMenuRequest request)
        {
            var dataResponse = _context.VwMenu
                                      .Where(v => v.RolId == request.RolId)
                                      .OrderBy(v => v.MenuOrden)
                                      .ThenBy(v => v.SubMenuOrden)
                                      .Select(v => new GetVwMenuResponse
                                      {
                                          PermisoId = v.PermisoId,
                                          ModuloId = v.ModuloId,
                                          ModuloIdPadre = v.ModuloIdPadre,
                                          Menu = v.Menu,
                                          MenuPath = v.MenuPath,
                                          SubMenu = v.SubMenu,
                                          SubMenuPath = v.SubMenuPath,
                                          Rol = v.Rol,
                                          RolId = v.RolId,
                                          ModuloTipo = v.ModuloTipo,
                                          ModuloTipoId = v.ModuloTipoId,
                                          MenuOrden = v.MenuOrden,
                                          SubMenuOrden = v.SubMenuOrden
                                      })
                                      .ToList();

            return dataResponse;
        }
    }
}
