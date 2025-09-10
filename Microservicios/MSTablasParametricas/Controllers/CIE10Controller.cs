using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class CIE10Controller : GenericController<TPCIE10, CIE10DTO>
    {
        public CIE10Controller(IGenericService<TPCIE10, CIE10DTO> service,
                               IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}
