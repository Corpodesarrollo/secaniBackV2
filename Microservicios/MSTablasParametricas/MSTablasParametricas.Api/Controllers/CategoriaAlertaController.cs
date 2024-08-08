using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaAlertaController : GenericController<TPCategoriaAlerta, GenericTPDTO>
    {
        public CategoriaAlertaController(IGenericService<TPCategoriaAlerta, GenericTPDTO> service) : base(service)
        {
        }
    }
}