using Core.DTOs;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{

    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class EAPBController : GenericController<TPEAPB, TPEAPBDto>
    {
        public EAPBController(IGenericService<TPEAPB, TPEAPBDto> service,
                               IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}
