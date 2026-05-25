using SISPRO.TRV.Entity;

namespace Core.Interfaces
{
    public interface ICurrentUserProvider
    {
        User CurrentUser { get; }
    }
}
