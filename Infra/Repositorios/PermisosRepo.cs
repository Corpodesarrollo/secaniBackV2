using Core.Interfaces.Repositorios;
using Core.request;
using Core.response;
using MSAuthentication.Core.DTOs;

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
            List<GetVwMenuResponse> response = new List<GetVwMenuResponse>();
            var dataResponse = _context.VwMenu
                                      .Where(v => v.RoleId == request.RoleId)
                                      .OrderBy(v => v.MenuOrden)
                                      .Select(v => new MenuDto
                                      {
                                          PermisoId = v.PermisoId,
                                          RoleId = v.RoleId,
                                          RoleNombre = v.RoleNombre,
                                          FuncionalidadNombre = v.FuncionalidadNombre,
                                          MenuId = v.MenuId,
                                          MenuNombre = v.MenuNombre,
                                          MenuPath = v.MenuPath,
                                          MenuIcon = v.MenuIcon,
                                          MenuOrden = v.MenuOrden,
                                          MenuIdPadre = v.MenuIdPadre,
                                          TieneSubMenu = v.TieneSubMenu
                                      })
                                      .ToList();

            // Iterate over the list
            foreach (var menu in dataResponse)
            {

                List<MenuDto> reponseListSub = new List<MenuDto>();
                var subMenusList = _context.VwSubMenu
                                        .Where(v => v.RoleId == request.RoleId && menu.MenuId == v.MenuIdPadre)
                                        .OrderBy(v => v.MenuOrden)
                                        .Select(v => new MenuDto
                                        {
                                            PermisoId = v.PermisoId,
                                            RoleId = v.RoleId,
                                            RoleNombre = v.RoleNombre,
                                            FuncionalidadNombre = v.FuncionalidadNombre,
                                            MenuId = v.MenuId,
                                            MenuNombre = v.MenuNombre,
                                            MenuPath = v.MenuPath,
                                            MenuIcon = v.MenuIcon,
                                            MenuOrden = v.MenuOrden,
                                            MenuIdPadre = v.MenuIdPadre,
                                            TieneSubMenu = v.TieneSubMenu
                                        })
                                        .ToList();
                // Add the top-level menu with its submenus to the response
                response.Add(new GetVwMenuResponse
                {
                    PermisoId = menu.PermisoId,
                    RoleId = menu.RoleId,
                    RoleNombre = menu.RoleNombre,
                    FuncionalidadNombre = menu.FuncionalidadNombre,
                    MenuId = menu.MenuId,
                    MenuNombre = menu.MenuNombre,
                    MenuPath = menu.MenuPath,
                    MenuIcon = menu.MenuIcon,
                    MenuOrden = menu.MenuOrden,
                    MenuIdPadre = menu.MenuIdPadre,
                    TieneSubMenu = menu.TieneSubMenu,
                    SubMenus = subMenusList
                });

            }

            return response;
        }
    }
}
