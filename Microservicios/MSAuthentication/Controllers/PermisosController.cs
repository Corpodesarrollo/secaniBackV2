using Core.Interfaces.Repositorios;
using Core.request;
using Core.response;
using Microsoft.AspNetCore.Mvc;
using System.Threading;


namespace MSAuthentication.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PermisosController : ControllerBase
    {
        private IPermisosRepo permisosRepo;


        public PermisosController(IPermisosRepo _permisos)
        {
            permisosRepo = _permisos;
        }
        [HttpPost("MenuXRolId")]
        public List<GetVwMenuResponse> MenuXRolId(GetVwMenuRequest request)
        {
            List<GetVwMenuResponse> response = new List<GetVwMenuResponse>();

            response = permisosRepo.MenuXRolId(request, cancellationToken: default);

            return response;
        }
    }
}
