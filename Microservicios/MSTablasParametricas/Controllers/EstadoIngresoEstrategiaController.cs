using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoIngresoEstrategiaController : GenericController<TPEstadoIngresoEstrategia, GenericTPDTO>
    {
        public EstadoIngresoEstrategiaController(IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> service) : base(service)
        {
        }
    }
}