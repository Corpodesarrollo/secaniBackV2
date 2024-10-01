using Core.DTOs;
using SISPRO.TRV.Entity;

namespace Core.Interfaces.Repositorios
{
    public interface IAuthRepo
    {
        Task<UserDto?> GetUser(User data);
    }
}
