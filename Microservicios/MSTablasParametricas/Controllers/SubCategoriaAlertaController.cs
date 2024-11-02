using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class SubCategoriaAlertaController : GenericController<TPSubCategoriaAlerta, SubCategoriaAlertaDTO>
    {
        public SubCategoriaAlertaController(IGenericService<TPSubCategoriaAlerta, SubCategoriaAlertaDTO> service) : base(service)
        {
        }
    }
}