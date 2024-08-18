namespace Core.Interfaces.MSTablasParametricas
{
    public interface IGenericService<T1, T2> where T1 : class where T2 : class
    {
        Task<T2> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<T2> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<T2> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<IEnumerable<T2>> GetAllAsync(CancellationToken cancellationToken);
        Task<(bool, T2)> AddAsync(T1 entity, CancellationToken cancellationToken);
        Task<(bool, T2)> UpdateAsync(T1 entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
    }
}
