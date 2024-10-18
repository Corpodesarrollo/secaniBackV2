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
    public class CausaInasistenciaController : GenericController<TPCausaInasistencia, GenericTPDTO>
    {
        public CausaInasistenciaController(IGenericService<TPCausaInasistencia, GenericTPDTO> service) : base(service)
        {
        }
    }
}