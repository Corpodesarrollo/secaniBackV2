
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IIpsRepo
    {
        Task<bool> LoadData();
        Task<TPIPSDto[]> GetAll();
        Task<TPIPSDto[]?> GetMunicipio(string codeMunicipio);
        Task<TPIPSDto[]?> Search(string cadena);
    }
}
