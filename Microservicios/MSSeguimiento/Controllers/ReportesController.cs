using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class ReportesController(
        IReporteDepuracionService depuracionService, 
        IReporteDinamicoNNAService dinamicoNNAService,
        IReporteDinamicoSeguimientoService dinamicoSeguimientoService,
        IReporteDetalleRegDepuradosService reporteDepuradosService) : ControllerBase
    {
        private readonly IReporteDepuracionService _depuracionService = depuracionService;
        private readonly IReporteDinamicoNNAService _dinamicoNNAService = dinamicoNNAService;
        private readonly IReporteDinamicoSeguimientoService _dinamicoSeguimientoService = dinamicoSeguimientoService;
        private readonly IReporteDetalleRegDepuradosService _reporteDepuradosService = reporteDepuradosService;

        [HttpGet("EstadoDepuracion")]
        public async Task<List<ReporteDepuracionDTO>> ReporteDepuracion(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _depuracionService.GetReporteDepuraciones(FechaInicial, FechaFinal);
        }

        [HttpGet("ReporteDinamicoNNA")]
        public async Task<List<ReporteDinamicoNNADTO>> ReporteDinamicoNNA(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _dinamicoNNAService.GetReporteDinamicoNNAAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }

        [HttpGet("ReporteDinamicoSeguimiento")]
        public async Task<List<ReporteDinamicoSeguimientoDTO>> ReporteDinamicoSeguimiento(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _dinamicoSeguimientoService.GetReporteDinamicoSeguimientoAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }

        [HttpGet("ReporteDetalleRegDepurados")]
        public async Task<List<ReporteDetalleRegDepuradosDTO>> ReporteDetalleRegDepurados(DateTime FechaInicial, DateTime FechaFinal, int TipoRegistro = 0)
        {
            return await _reporteDepuradosService.GetReporteDetalleRegDepuradosAsync(FechaInicial, FechaFinal, TipoRegistro, cancellationToken: default);
        }
    }
}
