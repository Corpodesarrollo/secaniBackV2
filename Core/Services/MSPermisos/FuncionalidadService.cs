using Core.DTOs.MSPermisos;
using Core.Interfaces.Repositorios.MSPermisos;
using Core.Modelos;
using Mapster;

namespace Core.Services.MSPermisos
{
    public class FuncionalidadService(IFuncionalidadRepository repository) : IFuncionalidadService
    {
        private readonly IFuncionalidadRepository _repository = repository;

        public async Task<(bool, FuncionalidadResponseDTO)> AddAsync(FuncionalidadRequestDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = entity.Adapt<TPFuncionalidad>();
            var (success, response) = await _repository.AddAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot add permiso");
            }
            return (success, response.Adapt<FuncionalidadResponseDTO>());
        }

        public async Task<bool> DeleteAsync(FuncionalidadResponseDTO entity, CancellationToken cancellationToken)
        {
            return await _repository.DeleteAsync(entity.Adapt<TPFuncionalidad>());
        }

        public async Task<IEnumerable<FuncionalidadResponseDTO>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);
            return entities.Adapt<IEnumerable<FuncionalidadResponseDTO>>();
        }

        public async Task<FuncionalidadResponseDTO> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return entity.Adapt<FuncionalidadResponseDTO>();
        }

        public async Task<(bool, FuncionalidadResponseDTO)> UpdateAsync(FuncionalidadResponseDTO entity, CancellationToken cancellationToken)
        {
            var newEntity = await _repository.GetByIdAsync(entity.Id, cancellationToken);
            if (newEntity == null)
            {
                return (false, null);
            }
            newEntity.Nombre = entity.Nombre;
            newEntity.Descripcion = entity.Descripcion;

            var (success, response) = await _repository.UpdateAsync(newEntity);
            if (!success)
            {
                throw new Exception("cannot update permiso");
            }
            return (success, response.Adapt<FuncionalidadResponseDTO>());
        }
    }
}
