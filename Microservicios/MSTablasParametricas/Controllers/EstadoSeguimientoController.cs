using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class EstadoSeguimientoController : GenericController<TPEstadoSeguimiento, GenericTPDTO>
    {
        public EstadoSeguimientoController(IGenericService<TPEstadoSeguimiento, GenericTPDTO> service,
                IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}