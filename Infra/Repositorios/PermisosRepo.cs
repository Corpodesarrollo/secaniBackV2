using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios;
using Core.request;
using Core.response;
using Microsoft.Extensions.Caching.Memory;
using MSAuthentication.Core.DTOs;

namespace Infra.Repositories
{
    public class PermisosRepo : IPermisosRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private string cacheKey = "Funcionalidad";
        private MemoryCacheEntryOptions cacheEntryOptions =
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120),
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

        public PermisosRepo(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public List<GetVwMenuResponse> MenuXRolId(GetVwMenuRequest request, CancellationToken cancellationToken)
        {
            var cacheKeyRoleId = cacheKey + request.RoleId;

            if (!_cache.TryGetValue(cacheKeyRoleId, out List<GetVwMenuResponse> response))
            {
                response = new List<GetVwMenuResponse>(); // Inicializar response si no está en caché

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

                foreach (var menu in dataResponse)
                {
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
                        MenuPath = menu.MenuPath ?? "",
                        MenuIcon = menu.MenuIcon ?? "",
                        MenuOrden = menu.MenuOrden,
                        MenuIdPadre = menu.MenuIdPadre,
                        TieneSubMenu = menu.TieneSubMenu,
                        SubMenus = subMenusList
                    });
                }

               
                _cache.Set(cacheKeyRoleId, response, cacheEntryOptions);
            }

            return response;
        }
    }
}
