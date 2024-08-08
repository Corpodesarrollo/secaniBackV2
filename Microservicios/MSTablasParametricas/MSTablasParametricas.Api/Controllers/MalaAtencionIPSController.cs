using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MalaAtencionIPSController : GenericController<TPMalaAtencionIPS, GenericTPDTO>
    {
        public MalaAtencionIPSController(IGenericService<TPMalaAtencionIPS, GenericTPDTO> service) : base(service)
        {
        }
    }
}