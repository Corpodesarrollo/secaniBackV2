
using Core.DTOs;

namespace Core.Interfaces
{
    public interface ICuidadorRepo
    {
        Task<IEnumerable<CuidadorDto>> GetAllCuidadores();
        Task<bool> SetUserCuidador(string id);
    }
}
