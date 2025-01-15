using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReporteDinamicoEAPBController(IReporteDinamicoEAPBService service): ControllerBase
    {
        private readonly IReporteDinamicoEAPBService _service = service;

        [HttpGet]
        public async Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _service.GetReporteDinamicoEAPBAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }
    }
}
