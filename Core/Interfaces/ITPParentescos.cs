using Core.DTOs;

namespace Core.Interfaces
{
    public interface ITPParentescos
    {
        Task<IEnumerable<TPParentescosDto>> GetAllAsync();
    }
}
