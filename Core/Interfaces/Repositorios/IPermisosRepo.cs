using Core.request;
using Core.response;

namespace Core.Interfaces.Repositorios
{
    public interface IPermisosRepo
    {
        public List<GetVwMenuResponse> MenuXRolId(GetVwMenuRequest request);
    }
}
