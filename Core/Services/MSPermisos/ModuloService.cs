using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Mapster;

namespace Core.Services.MSPermisos
{
    public class ModuloService(IModuloRepository repository) : IModuloService
    {
        private readonly IModuloRepository _repository = repository;

        public async Task<(bool, ModuloResponseDTO)> AddAsync(ModuloRequestDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = entity.Adapt<TPModuloComponenteObjeto>();
            var (success, response) = await _repository.AddAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot add modulo");
            }
            return (success, response.Adapt<ModuloResponseDTO>());
        }

        public async Task<bool> DeleteAsync(ModuloResponseDTO entity, CancellationToken cancellationToken)
        {
            return await _repository.DeleteAsync(entity.Adapt<TPModuloComponenteObjeto>());
        }

        public async Task<IEnumerable<ModuloResponseDTO>> GetAllAsync(CancellationToken cancellationToken)
        {
            try
            {
                var entities = await _repository.GetAllAsync(cancellationToken);
                return entities.Adapt<IEnumerable<ModuloResponseDTO>>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<ModuloResponseDTO> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return entity.Adapt<ModuloResponseDTO>();
        }

        public async Task<(bool, ModuloResponseDTO)> UpdateAsync(ModuloResponseDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = await _repository.GetByIdAsync(entity.Id, cancellationToken);
            if (newEntity == null)
            {
                return (false, null);
            }
            newEntity.Icon = entity.Icon;
            newEntity.Nombre = entity.Nombre;
            newEntity.Path = entity.Path;
            newEntity.ModuloComponenteObjetoIdPadre = entity.ModuloComponenteObjetoIdPadre ?? 0;
            newEntity.Orden = entity.Orden;
            newEntity.Activo = entity.Activo;

            var (success, response) = await _repository.UpdateAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot update modulo");
            }
            return (success, response.Adapt<ModuloResponseDTO>());
        }
    }
}
