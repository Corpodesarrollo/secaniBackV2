using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class TipoFallaLlamadaController : GenericController<TPTipoFallaLlamada, GenericTPDTO>
    {
        public TipoFallaLlamadaController(IGenericService<TPTipoFallaLlamada, GenericTPDTO> service,
                                           IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}