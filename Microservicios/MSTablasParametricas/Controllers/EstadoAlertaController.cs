using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class EstadoAlertaController : GenericController<TPEstadoAlerta, GenericTPDTO>
    {
        private readonly IGenericService<TPEstadoAlerta, GenericTPDTO> _service;
        private readonly IHistoricoTransaccionService _historicoService;
        public EstadoAlertaController(IGenericService<TPEstadoAlerta, GenericTPDTO> service, IHistoricoTransaccionService historicoService
                                     ) : base(service, historicoService)
        {
            _service = service;
            _historicoService = historicoService;
        }

    }
}
