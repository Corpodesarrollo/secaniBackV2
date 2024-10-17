using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class TipoFallaLlamadaController : GenericController<TPTipoFallaLlamada, GenericTPDTO>
    {
        public TipoFallaLlamadaController(IGenericService<TPTipoFallaLlamada, GenericTPDTO> service) : base(service)
        {
        }
    }
}