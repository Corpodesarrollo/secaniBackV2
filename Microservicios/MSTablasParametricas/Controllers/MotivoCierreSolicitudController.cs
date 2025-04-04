using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class MotivoCierreSolicitudController : GenericController<TPMotivoCierreSolicitud, GenericTPDTO>
    {
        public MotivoCierreSolicitudController(IGenericService<TPMotivoCierreSolicitud, GenericTPDTO> service,
                                                IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}