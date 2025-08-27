
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IIpsRepo
    {
        Task<bool> LoadData();
        Task<TPIPSDto[]> GetAll();
        Task<TPIPSDto[]?> GetMunicipio(string codeMunicipio);
        Task<TPIPSDto[]?> Search(string cadena);
        Task<TPIPSDto?> GetIPSByCode(string code);
        Task<TPIPSDto?> GetById(long id);
    }
}
