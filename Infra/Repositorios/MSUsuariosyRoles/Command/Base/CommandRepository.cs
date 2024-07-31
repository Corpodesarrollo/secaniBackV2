using Core.Interfaces.Repositorios.MSUsuariosyRoles.Command.Base;
using Infra.MSUsuariosyRoles;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.MSUsuariosyRoles.Command.Base
{
    public class CommandRepository<T> : ICommandRepository<T> where T : class
    {
        protected readonly ApplicationIdentityDbContext _context;

        public CommandRepository(ApplicationIdentityDbContext context)
        {
            _context = context;
        }

        // Insert
        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // Update
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
