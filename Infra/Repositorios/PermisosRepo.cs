using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Core.request;
using Core.response;
using Mapster;
using Microsoft.Extensions.Caching.Memory;
using MSAuthentication.Core.DTOs;

namespace Infra.Repositories
{
    public class PermisosRepo : IPermisosRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly IPermisoRepository _repository;
        private readonly IMemoryCache _cache;
        private string cacheKey = "Funcionalidad";
        private MemoryCacheEntryOptions cacheEntryOptions =
            new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120),
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };

        public PermisosRepo(IPermisoRepository repository, ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
            _repository = repository;
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

        public async Task<IEnumerable<PermisoResponseDTO>> GetAllAsync(CancellationToken cancellationToken)
        {
            if (!_cache.TryGetValue(cacheKey, out List<PermisoResponseDTO> entitiesDto))
            {
                var entities = await _repository.GetPermisos(cancellationToken);
                entitiesDto = entities.Adapt<List<PermisoResponseDTO>>();
                foreach (var entity in entities)
                {
                    var (permiso, funcionalidad, modulo) = await _repository.GetPermisoWithFuncionalidadAndModuloById(entity.Id, cancellationToken);
                    var permisoDto = permiso.Adapt<PermisoResponseDTO>();
                    permisoDto.Funcionalidad = funcionalidad;
                    permisoDto.Modulo = modulo;
                    entitiesDto.Add(permisoDto);
                }
                _cache.Set(cacheKey, entitiesDto, cacheEntryOptions);
            }
            return entitiesDto;
        }

        public async Task<IEnumerable<PermisoResponseDTO>> GetAllByRoleIdAsync(string RoleId, CancellationToken cancellationToken)
        {
            var cacheKeyRoleId = cacheKey + RoleId;
            if (!_cache.TryGetValue(cacheKeyRoleId, out List<PermisoResponseDTO> entitiesDto))
            {
                var entities = await _repository.GetPermisosByRoleId(RoleId, cancellationToken);
                entitiesDto = new List<PermisoResponseDTO>();
                foreach (var entity in entities)
                {
                    var (permiso, funcionalidad, modulo) = await _repository.GetPermisoWithFuncionalidadAndModuloById(entity.Id, cancellationToken);
                    var permisoDto = permiso.Adapt<PermisoResponseDTO>();
                    permisoDto.Funcionalidad = funcionalidad;
                    permisoDto.Modulo = modulo;
                    entitiesDto.Add(permisoDto);
                }
                _cache.Set(cacheKeyRoleId, entitiesDto, cacheEntryOptions);
            }
            return entitiesDto;
        }

        public async Task<IEnumerable<PermisoResponseDTO>> GetAllByModuloIdAsync(int ModuloId, CancellationToken cancellationToken)
        {
            var cacheKeyModuloId = cacheKey + ModuloId;
            if (!_cache.TryGetValue(cacheKeyModuloId, out List<PermisoResponseDTO> entitiesDto))
            {
                var entities = await _repository.GetPermisosByModuloId(ModuloId, cancellationToken);
                entitiesDto = new List<PermisoResponseDTO>();
                foreach (var entity in entities)
                {
                    var (permiso, funcionalidad, modulo) = await _repository.GetPermisoWithFuncionalidadAndModuloById(entity.Id, cancellationToken);
                    var permisoDto = permiso.Adapt<PermisoResponseDTO>();
                    permisoDto.Funcionalidad = funcionalidad;
                    permisoDto.Modulo = modulo;
                    entitiesDto.Add(permisoDto);
                }
                _cache.Set(cacheKeyModuloId, entitiesDto, cacheEntryOptions);
            }
            return entitiesDto;
        }

        public async Task<IEnumerable<PermisoResponseDTO>> GetAllByModuloandRoleAsync(string RoleId, int ModuloId, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetPermisosByRoleandModulo(RoleId, ModuloId, cancellationToken);
            var entitiesDto = new List<PermisoResponseDTO>();
            foreach (var entity in entities)
            {
                var (permiso, funcionalidad, modulo) = await _repository.GetPermisoWithFuncionalidadAndModuloById(entity.Id, cancellationToken);
                var permisoDto = permiso.Adapt<PermisoResponseDTO>();
                permisoDto.Funcionalidad = funcionalidad;
                permisoDto.Modulo = modulo;
                entitiesDto.Add(permisoDto);
            }

            return entitiesDto;
        }

        public async Task<PermisoResponseDTO> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            var cacheKeyId = cacheKey + id.ToString();
            if (!_cache.TryGetValue(cacheKeyId, out PermisoResponseDTO entityDto))
            {
                var entity = await _repository.GetByIdAsync(id, cancellationToken);
                var (permiso, funcionalidad, modulo) = await _repository.GetPermisoWithFuncionalidadAndModuloById(entity.Id, cancellationToken);
                entityDto = permiso.Adapt<PermisoResponseDTO>();
                entityDto.Funcionalidad = funcionalidad;
                entityDto.Modulo = modulo;
                _cache.Set(cacheKeyId, entityDto, cacheEntryOptions);
            }
            return entityDto;
        }

        //Commands
        public async Task<(bool, PermisoResponseDTO)> AddAsync(PermisoRequestDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = entity.Adapt<Permisos>();
            var (success, response) = await _repository.AddAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot add permiso");
            }
            await ClearCacheAsync(cacheKey, newEntity.RoleId ?? null);
            return (success, response.Adapt<PermisoResponseDTO>());
        }

        public async Task<bool> DeleteAsync(PermisoResponseDTO entity, CancellationToken cancellationToken)
        {
            await ClearCacheAsync(cacheKey, entity.RoleId, entity.Id.ToString());
            return await _repository.DeleteAsync(entity.Adapt<Permisos>());
        }

        public async Task<(bool, PermisoResponseDTO)> UpdateAsync(PermisoResponseDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = await _repository.GetByIdAsync(entity.Id, cancellationToken);
            if (newEntity == null)
            {
                return (false, null);
            }

            await ClearCacheAsync(cacheKey, entity.RoleId, entity.Id.ToString());

            newEntity.ModuloComponenteObjetoId = entity.ModuloComponenteObjetoId;
            newEntity.RoleId = entity.RoleId;
            newEntity.FuncionalidadId = entity.FuncionalidadId;
            newEntity.CanAdd = entity.CanAdd;
            newEntity.CanEdit = entity.CanEdit;
            newEntity.CanDele = entity.CanDele;
            newEntity.CanView = entity.CanView;

            var (success, response) = await _repository.UpdateAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot update permiso");
            }

            return (success, response.Adapt<PermisoResponseDTO>());
        }
        public async Task ClearCacheAsync(string cacheKey, string roleId = null, string id = null)
        {
            _cache.Remove(cacheKey);
            if (roleId != null)
            {
                _cache.Remove(cacheKey + roleId);
            }
            if (id != null)
            {
                _cache.Remove(cacheKey + id);
            }
        }
    }
}
