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
    public class OrigenReporteController : GenericController<TPOrigenReporte, GenericTPDTO>
    {
        public OrigenReporteController(IGenericService<TPOrigenReporte, GenericTPDTO> service) : base(service)
        {
        }
    }
}