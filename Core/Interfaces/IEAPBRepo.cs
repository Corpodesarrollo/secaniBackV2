
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IEAPBRepo
    {
        Task<List<TPEAPBDto>> Entidates();
        Task<List<TPEAPBDto>> GetEAPB();
        Task<TPEAPBDto?> GetEAPBByCode(string code);
        Task<TPEAPBDto?> GetEAPBByCodeOrId(string value);
        Task<TPEAPBDto?> GetEAPBById(int id);
        Task<List<TPEAPBDto>> GetET();
        Task<List<TPEAPBDto>> Search(string cadena);
    }
}
