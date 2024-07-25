using Core.Interfaces.Repositorios;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ContactoEntidadRepository : GenericRepository<ContactoEntidad>, IContactoEntidadRepository
    {
        public ContactoEntidadRepository(ApplicationDbContext _context) : base(_context)
        {
        }

        public async Task<List<ContactoEntidad>> GetAllContactosEntidad(CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.Include(x => x.Entidad).Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<ContactoEntidad> GetContactoEntidadByEmail(string email, CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.Include(x => x.Entidad).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<ContactoEntidad> GetContactoEntidadById(long id, CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.Include(x => x.Entidad).FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
