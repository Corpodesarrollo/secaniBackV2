using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;

namespace Core.Interfaces.Repositorios
{
    public interface IContactoNNARepo
    {


        Task<ContactoNNA> GetByIdAsync(long id);
        Task<List<ContactoNNA>> FindAllAsync(long NNAId);

        Task<(bool, ContactoNNA)> AddAsync(ContactoNNA entity);
        Task<(bool, ContactoNNA)> UpdateAsync(ContactoNNA entity);


    }
}
