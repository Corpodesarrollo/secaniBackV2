using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Core.Modelos.TablasParametricas;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoriaAlertaController : GenericController<TPSubCategoriaAlerta, SubCategoriaAlertaDTO>
    {
        public SubCategoriaAlertaController(IGenericService<TPSubCategoriaAlerta, SubCategoriaAlertaDTO> service) : base(service)
        {
        }
    }
}