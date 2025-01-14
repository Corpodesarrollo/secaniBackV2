using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReporteGeneralLlamadasController(IReporteGeneralLlamadasService service): ControllerBase
    {
        private readonly IReporteGeneralLlamadasService _service = service;

        [HttpGet]
        public async Task<List<ReporteGeneralLlamadasDTO>> GetReporteGeneralLlamadasAsync(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _service.GetReporteGeneralLlamadasAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }
    }
}
