using Core.Interfaces.Repositorios;
using Core.Modelos;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class ContactoEntidadRepository : GenericRepository<ContactoEntidades>, IContactoEntidadRepository
    {
        public ContactoEntidadRepository(ApplicationDbContext _context) : base(_context)
        {
        }

        public async Task<List<ContactoEntidades>> GetAllContactosEntidad(CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<ContactoEntidades> GetContactoEntidadByEmail(string email, CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<ContactoEntidades> GetContactoEntidadById(long id, CancellationToken cancellationToken)
        {
            return await _context.ContactoEntidades.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
