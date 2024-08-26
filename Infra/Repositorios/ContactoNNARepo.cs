using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Infra.Repositories.Common;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class ContactoNNARepo : IContactoNNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<ContactoNNA> _repository;

        public ContactoNNARepo(ApplicationDbContext context)
        {
            _context = context;
            GenericRepository<ContactoNNA> repository = new GenericRepository<ContactoNNA>(_context);
            _repository = repository;
        }


        public async Task<ContactoNNA> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<List<ContactoNNA>> FindAllAsync(long NNAId)
        {
            // Call the repository method with the correct parameters
            var response = await _repository.FindAllAsync(p => p.NNAId == NNAId);

            // Process the results as needed
            return response.ToList(); // Convert the IEnumerable to List
        }


        public async Task<(bool, ContactoNNA)> AddAsync(ContactoNNA entity)
        {
            var (success, response) = await _repository.AddAsync(entity);
            if (!success)
            {
                throw new Exception("cannot add entity");
            }
            return (success, response);
        }

        public async Task<(bool, ContactoNNA)> UpdateAsync(ContactoNNA entity)
        {
            var (success, response) = await _repository.UpdateAsync(entity);
            if (!success)
            {
                throw new Exception("cannot update entity");
            }
            // Adaptar la respuesta de NNAs a NNADto
            //var result = response.Adapt<ContactoNNADto>();
            return (success, response);
        }



    }

}