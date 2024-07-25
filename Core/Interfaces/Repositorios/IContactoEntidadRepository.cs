using Core.Interfaces.Repositorios.Common;
using Core.Modelos;

namespace Core.Interfaces.Repositorios
{
    public interface IContactoEntidadRepository : IGenericRepository<ContactoEntidad>
    {
        public Task<ContactoEntidad> GetContactoEntidadByEmail(string email, CancellationToken cancellationToken);
        public Task<ContactoEntidad> GetContactoEntidadById(long id, CancellationToken cancellationToken);
        public Task<List<ContactoEntidad>> GetAllContactosEntidad(CancellationToken cancellationToken);
    }
}
