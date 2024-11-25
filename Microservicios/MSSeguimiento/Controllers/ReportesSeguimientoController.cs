using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class ReportesSeguimientoController(IReporteDepuracionService service, IReporteDinamicoNNAService serviceDinamico) : ControllerBase
    {
        private readonly IReporteDepuracionService _service = service;
        private readonly IReporteDinamicoNNAService _serviceDinamico = serviceDinamico;

        [HttpGet("EstadoDepuracion")]
        public async Task<List<ReporteDepuracionDTO>> ReporteDepuracion(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _service.GetReporteDepuraciones(FechaInicial, FechaFinal);
        }

        [HttpGet("ReporteDinamicoNNA")]
        public async Task<List<ReporteDinamicoNNADTO>> ReporteDinamicoNNA(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _serviceDinamico.GetReporteDinamicoNNAAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }
    }
}
