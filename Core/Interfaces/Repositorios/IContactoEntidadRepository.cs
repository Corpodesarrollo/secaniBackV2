using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios
{
    public interface IContactoEntidadRepository : IGenericRepository<ContactoEntidades>
    {
        public Task<ContactoEntidades> GetContactoEntidadByEmail(string email, CancellationToken cancellationToken);
        public Task<ContactoEntidades> GetContactoEntidadById(long id, CancellationToken cancellationToken);
        public Task<List<ContactoEntidades>> GetAllContactosEntidad(CancellationToken cancellationToken);
    }
}
