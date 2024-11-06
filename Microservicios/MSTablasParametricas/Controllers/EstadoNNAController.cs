using Core.DTOs;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class EstadoNNAController(IGenericService<TPEstadoNNA, TPEstadoNNADto> service) : GenericController<TPEstadoNNA, TPEstadoNNADto>(service)
    {
    }
}
