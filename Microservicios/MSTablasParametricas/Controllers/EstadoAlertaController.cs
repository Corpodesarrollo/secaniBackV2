using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{

    [Route("api/[controller]")]
    //[Authorize]
    [ApiController]
    public class EstadoAlertaController : GenericController<TPEstadoAlerta, GenericTPDTO>
    {
        public EstadoAlertaController(IGenericService<TPEstadoAlerta, GenericTPDTO> service) : base(service)
        {
        }
    }
}
