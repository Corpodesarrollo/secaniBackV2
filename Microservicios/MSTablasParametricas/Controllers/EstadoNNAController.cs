using Core.DTOs;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class EstadoNNAController(IGenericService<TPEstadoNNA, TPEstadoNNADto> service, IHistoricoTransaccionService historicoService)
        : GenericController<TPEstadoNNA, TPEstadoNNADto>(service, historicoService)
    {
    }
}
