using Core.Interfaces.Repositorios.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Infra.Repositories.Common
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, string includeTable = "")
        {
            var result = await Task.Run(() => _context.Set<T>().Where<T>(predicate));
            if (includeTable != "")
                result = result.Include(includeTable);
            return result;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            var items = await _context.Set<T>().AsNoTracking().ToListAsync();
            return items ?? Enumerable.Empty<T>();
        }
        public async Task<IEnumerable<T>> GetAllNotDeletedAsync(CancellationToken cancellationToken)
        {
            var prop = typeof(T).GetProperty("IsDeleted");
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                // Construye una expresión lambda: x => x.IsDeleted == false
                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, prop);
                var falseConstant = Expression.Constant(false);
                var equalExpression = Expression.Equal(propertyAccess, falseConstant);
                var lambda = Expression.Lambda<Func<T, bool>>(equalExpression, parameter);

                return await _context.Set<T>()
                    .AsNoTracking()
                    .Where(lambda)
                    .ToListAsync(cancellationToken);
            }

            // Si no tiene IsDeleted, retorna todo
            return await _context.Set<T>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }


        public async Task<T> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> GetByIdAsync(long id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<T> GetByIdAsync(string id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task<(int totalRegistros, IEnumerable<T> registros)> GetByPageAsync(int pageIndex, int pageSize, string search, bool Ascending, PropertyInfo propertyInfo)
        {
            var totalRegistros = await _context.Set<T>().CountAsync();
            IEnumerable<T> registros;

            if (Ascending)
            {

                registros = await _context.Set<T>()
                                    .OrderBy(x => propertyInfo.GetValue(x, null))
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            }
            else
            {
                registros = await _context.Set<T>()
                                    .OrderByDescending(x => propertyInfo.GetValue(x, null))
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();
            }

            return (totalRegistros, registros);
        }

        public async Task<(bool, T)> AddAsync(T entity)
        {
            try
            {
                await _context.Set<T>().AddAsync(entity);
                var result = await _context.SaveChangesAsync();
                return (result > 0, entity);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return (false, null);
        }

        // Update
        public async Task<(bool, T)> UpdateAsync(T entity)
        {
            var keyProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

            if (keyProperty == null)
            {
                throw new InvalidOperationException($"No se encontró una propiedad 'Id' en {typeof(T).Name}.");
            }

            var keyValue = keyProperty.GetValue(entity);

            var existingEntity = await _context.Set<T>().FindAsync(keyValue);

            if (existingEntity == null)
            {
                return (false, null); // O lanza una excepción si prefieres
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            var result = await _context.SaveChangesAsync();
            return (result > 0, existingEntity);
        }



        // Delete
        public async Task<bool> DeleteAsync(T entity)
        {
            var prop = typeof(T).GetProperty("IsDeleted");

            if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
            {
                try
                {
                    prop.SetValue(entity, true);
                    _context.Entry(entity).State = EntityState.Modified;
                    var result = await _context.SaveChangesAsync();
                    return result > 0;
                }
                catch
                {
                    return false;
                }
            }

            return false; // No tiene propiedad IsDeleted
        }
    }
}
