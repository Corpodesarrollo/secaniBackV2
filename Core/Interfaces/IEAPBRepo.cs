
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IEAPBRepo
    {
        Task<List<TPEAPBDto>> GetEAPB();
        Task<List<TPEAPBDto>> GetET();
    }
}
