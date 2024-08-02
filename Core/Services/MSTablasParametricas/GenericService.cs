using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Common;
using Mapster;

namespace Core.Services.MSTablasParametricas
{
    public class GenericService<T1, T2>: IGenericService<T1,T2> where T1 : class where T2 : class 
    {
        private readonly IGenericRepository<T1> _repository;

        public GenericService(IGenericRepository<T1> repository)
        {
            _repository = repository;
        }

        public async Task<T2> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return entity.Adapt<T2>();
        }

        public async Task<T2> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            return entity.Adapt<T2>();
        }

        public async Task<IEnumerable<T2>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllAsync(cancellationToken);
            return entities.Adapt<IEnumerable<T2>>();
        }

        public async Task<(bool, T2)> AddAsync(T1 entity, CancellationToken cancellationToken)
        {
            var (success, response) = await _repository.AddAsync(entity);
            if (!success)
            {
                throw new Exception("cannot add entity");
            }
            return (success, response.Adapt<T2>());
        }

        public async Task<(bool, T2)> UpdateAsync(T1 entity, CancellationToken cancellationToken)
        {
            var (success, response) = await _repository.UpdateAsync(entity);
            if (!success)
            {
                throw new Exception("cannot update entity");
            }
            return (success, response.Adapt<T2>());
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return false;
            }
            return await _repository.DeleteAsync(entity);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return false;
            }
            return await _repository.DeleteAsync(entity);
        }
    }

}
