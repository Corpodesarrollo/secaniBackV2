using Core.Interfaces.Repositorios;
using Core.Request;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeguimientoController : ControllerBase
    {
        private readonly ISeguimientoRepo seguimientoRepo;

        public SeguimientoController(ISeguimientoRepo seguimientoRepo)
        {
            this.seguimientoRepo = seguimientoRepo;
        }

        [HttpPost("SetEstadoDiagnosticoTratamiento")]
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetEstadoDiagnosticoTratamiento(request);
        }

        [HttpPost("SetDiagnosticoTratamiento")]
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetDiagnosticoTratamiento(request);
        }

        [HttpPost("SetResidenciaDiagnosticoTratamiento")]
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetResidenciaDiagnosticoTratamiento(request);
        }
    }
}
