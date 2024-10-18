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
    public class CIE10Controller : GenericController<TPCIE10, CIE10DTO>
    {
        public CIE10Controller(IGenericService<TPCIE10, CIE10DTO> service) : base(service)
        {
        }
    }
}
