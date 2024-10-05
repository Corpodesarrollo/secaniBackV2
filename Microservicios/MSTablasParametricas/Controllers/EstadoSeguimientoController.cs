using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadoSeguimientoController : GenericController<TPEstadoSeguimiento, GenericTPDTO>
    {
        public EstadoSeguimientoController(IGenericService<TPEstadoSeguimiento, GenericTPDTO> service) : base(service)
        {
        }
    }
}