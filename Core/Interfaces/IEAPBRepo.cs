
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IEAPBRepo
    {
        Task<List<TPEAPBDto>> Entidates();
        Task<List<TPEAPBDto>> GetEAPB();
        Task<TPEAPBDto?> GetEAPBByCode(string code);
        Task<List<TPEAPBDto>> GetET();
        Task<List<TPEAPBDto>> Search(string cadena);
    }
}
