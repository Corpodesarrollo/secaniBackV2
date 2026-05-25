using Core.DTOs;

namespace Core.Interfaces
{
    public interface IEmailConfigurationRepo
    {
        Task<EmailConfigurationDto?> Get();
    }
}
