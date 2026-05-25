using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class CausaInasistenciaController : GenericController<TPCausaInasistencia, GenericTPDTO>
    {
        public CausaInasistenciaController(IGenericService<TPCausaInasistencia, GenericTPDTO> service,
                                            IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}