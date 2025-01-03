using Core.DTOs;
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
        IReporteDetalleRegDepuradosService reporteDepuradosService,
        IReporteDinamicoAlertasService reporteDinamicoAlertasService) : ControllerBase
    {
        private readonly IReporteDepuracionService _depuracionService = depuracionService;
        private readonly IReporteDinamicoNNAService _dinamicoNNAService = dinamicoNNAService;
        private readonly IReporteDinamicoSeguimientoService _dinamicoSeguimientoService = dinamicoSeguimientoService;
        private readonly IReporteDetalleRegDepuradosService _reporteDepuradosService = reporteDepuradosService;
        private readonly IReporteDinamicoAlertasService _reporteDinamicoAlertasService = reporteDinamicoAlertasService;

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

        [HttpGet("EstadoDepuracion")]
        public async Task<List<ReporteDepuracionDTO>> ReporteDepuracion(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _depuracionService.GetReporteDepuraciones(FechaInicial, FechaFinal);
        }

        [HttpGet("ReporteDetalleRegDepurados")]
        public async Task<List<ReporteDetalleRegDepuradosDTO>> ReporteDetalleRegDepurados(int IdReporteDepuracion, int TipoRegistro = 1)
        {
            return await _reporteDepuradosService.GetReporteDetalleRegDepuradosAsync(IdReporteDepuracion, TipoRegistro, cancellationToken: default);
        }

        [HttpGet("ReporteDinamicoAlertas")]
        public async Task<List<ReporteDinamicoAlertasDTO>> ReporteDinamicoAlertas(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _reporteDinamicoAlertasService.GetReporteDinamicoAlertasAsync(FechaInicial, FechaFinal, default);
        }

        [HttpPost("ReporteCasosEAPB")]
        public async Task<List<ReporteCasosEAPBDto>> ReporteCasosEAPB(ReporteCasosEAPBRequestDto request)
        {
            return await _dinamicoSeguimientoService.GetReporteCasosEAPBAsync(request);
        }

        [HttpPost("ReporteCasosEAPBExcel")]
        public async Task<string> ReporteCasosEAPBExcel(ReporteCasosEAPBRequestDto request)
        {
            return await _dinamicoSeguimientoService.GetReporteCasosEAPBExcelAsync(request);
        }

        [HttpPost("ReporteCasosEntidad")]
        public async Task<List<ReporteCasosEAPBDto>> ReporteCasosEntidad(ReporteCasosEntidadRequestDto request)
        {
            return await _dinamicoSeguimientoService.GetReporteCasosEntidadAsync(request);
        }

        [HttpPost("ReporteCasosEntidadExcel")]
        public async Task<string> ReporteCasosEAPBExcel(ReporteCasosEntidadRequestDto request)
        {
            return await _dinamicoSeguimientoService.GetReporteCasosEntidadExcelAsync(request);
        }
    }
}
