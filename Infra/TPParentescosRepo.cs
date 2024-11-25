using Core.Interfaces.Repositorios;
using Core.Modelos;
using Infra.Repositories.Common;

namespace Infra
{
    public class TPParentescosRepo(ApplicationDbContext _context) : GenericRepository<ContactoNNA>(_context), IContactoNNARepository
    {
    }
}
