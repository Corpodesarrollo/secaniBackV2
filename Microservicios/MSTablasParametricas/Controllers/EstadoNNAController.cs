using Core.DTOs;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class EstadoNNAController(IGenericService<TPEstadoNNA, TPEstadoNNADto> service, IHistoricoTransaccionService historicoService)
        : GenericController<TPEstadoNNA, TPEstadoNNADto>(service, historicoService)
    {
    }
}
