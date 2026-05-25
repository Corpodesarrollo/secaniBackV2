using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{
    public class SubCategoriaAlertaController : GenericController<TPSubCategoriaAlerta, SubCategoriaAlertaDTO>
    {
        public SubCategoriaAlertaController(IGenericService<TPSubCategoriaAlerta, SubCategoriaAlertaDTO> service,
                                           IHistoricoTransaccionService historicoService) : base(service, historicoService)
        {
        }
    }
}