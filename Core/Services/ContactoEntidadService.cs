using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Mapster;
using System.Reflection;

namespace Core.Services
{
    public class ContactoEntidadService(IContactoEntidadRepository repository) : IContactoEntidadService
    {
        private readonly IContactoEntidadRepository _repository = repository;

        public async Task<(bool, ContactoEntidadResponse)> AddAsync(ContactoEntidadRequest entity, CancellationToken cancellationToken)
        {
            var contactoEntidad = entity.Adapt<ContactoEntidad>();
            var result = await _repository.AddAsync(contactoEntidad);
            return result.Adapt<(bool, ContactoEntidadResponse)>();
        }

        public async Task<(bool, ContactoEntidadResponse)> UpdateAsync(long id, ContactoEntidadRequest request, CancellationToken cancellationToken)
        {
            var contactoEntidad = await _repository.GetByIdAsync(id, cancellationToken);

            if (contactoEntidad == null)
            {
                throw new Exception($"Contacto Entidad con identificar {id} not found");
            }

            contactoEntidad.Nombres = request.Nombres;
            contactoEntidad.EntidadId = request.EntidadId;
            contactoEntidad.Cargo = request.Cargo;
            contactoEntidad.Email = request.Email;
            contactoEntidad.Telefonos = request.Telefonos;
            contactoEntidad.Estado = request.Estado;

            var result = await _repository.UpdateAsync(contactoEntidad);
            return result.Adapt<(bool, ContactoEntidadResponse)>();
        }

        public async Task<bool> DeleteAsync(ContactoEntidadResponse entity, CancellationToken cancellationToken)
        {
            var contactoEntidad = entity.Adapt<ContactoEntidad>();
            contactoEntidad.IsDeleted = true;
            var (isDeleted, _) = await _repository.UpdateAsync(contactoEntidad);
            return isDeleted;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var contactoEntidad = await _repository.GetByIdAsync(id, cancellationToken: default);
            if (contactoEntidad == null)
            {
                return false;
            }
            contactoEntidad.IsDeleted = true;
            var (isDeleted, _) = await _repository.UpdateAsync(contactoEntidad);
            return isDeleted;
        }

        public async Task<IEnumerable<ContactoEntidadResponse>> GetAllAsync(CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllAsync(cancellationToken);
            return result.Adapt<IEnumerable<ContactoEntidadResponse>>();
        }

        public async Task<ContactoEntidadResponse> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return null;
            }
            return result.Adapt<ContactoEntidadResponse>();
        }

        public async Task<(int totalRegistros, IEnumerable<ContactoEntidadResponse> registros)> GetByPageAsync(int pageIndex, int pageSize, string search, bool Ascending, PropertyInfo propertyInfo)
        {
            var result = await _repository.GetByPageAsync(pageIndex, pageSize, search, Ascending, propertyInfo);
            return result.Adapt<(int totalRegistros, IEnumerable<ContactoEntidadResponse> registros)>();
        }

        public async Task<ContactoEntidadResponse> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var result = await _repository.GetContactoEntidadByEmail(email, cancellationToken);
            if (result == null)
            {
                return null;
            }
            return result.Adapt<ContactoEntidadResponse>();
        }
    }
}
