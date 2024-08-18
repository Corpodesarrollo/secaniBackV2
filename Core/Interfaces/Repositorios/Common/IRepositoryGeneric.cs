using System.Linq.Expressions;
using System.Reflection;

namespace Core.Interfaces.Repositorios.Common
{
    public interface IGenericRepository<T> where T : class
    {
        //Queries
        Task<T> GetByIdAsync(long id, CancellationToken cancellationToken);
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<T> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, string includeTable = "");
        Task<(int totalRegistros, IEnumerable<T> registros)> GetByPageAsync(int pageIndex, int pageSize, string search, bool Ascending, PropertyInfo propertyInfo);


        //Commands
        Task<(bool, T)> AddAsync(T entity);
        Task<(bool, T)> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
    }
}
