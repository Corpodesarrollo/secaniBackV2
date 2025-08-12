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

        [HttpGet("ReporteCasosEAPB/{fechaInicial}/{fechaFinal}/{idEAPB}")]
        public async Task<List<ReporteCasosEAPBDto>> ReporteCasosEAPB(DateTime fechaInicial, DateTime fechaFinal, int idEAPB)
        {
            var request = new ReporteCasosEAPBRequestDto
            {
                FechaInicial = fechaInicial,
                FechaFinal = fechaFinal,
                EAPB = idEAPB,
                DiagnosticoNNA = true,
                AreaProcedencia = true,
                CategoriaAlerta = true,
                DepartamentoProcedencia = true,
                DireccionProcedencia = true,
                SubcategoriaAlerta = true,
                Respuesta = true,
                MunicipioProcedencia = true,
                DepartamentoActual = true,
                Observaciones = true,
                TipoIdentificacion = true,
                BarrioProcedencia = true,
                NumeroIdentificacion = true,
                RegimenAfiliacion = true,
                EstadoNNA = true,
                IdCicloVida = 1,
                FechaEnvioRespuesta = true
            };

            return await _dinamicoSeguimientoService.GetReporteCasosEAPBAsync(request);
        }

        [HttpGet("ReporteCasosEAPBExcel/{fechaInicial}/{fechaFinal}/{idEAPB}")]
        public async Task<string> ReporteCasosEAPBExcel(DateTime fechaInicial, DateTime fechaFinal, int idEAPB)
        {
            var request = new ReporteCasosEAPBRequestDto
            {
                FechaInicial = fechaInicial,
                FechaFinal = fechaFinal,
                EAPB = idEAPB,
                DiagnosticoNNA = true,
                AreaProcedencia = true,
                CategoriaAlerta = true,
                DepartamentoProcedencia = true,
                DireccionProcedencia = true,
                SubcategoriaAlerta = true,
                Respuesta = true,
                MunicipioProcedencia = true,
                DepartamentoActual = true,
                Observaciones = true,
                TipoIdentificacion = true,
                BarrioProcedencia = true,
                NumeroIdentificacion = true,
                RegimenAfiliacion = true,
                EstadoNNA = true,
                IdCicloVida = 1,
                FechaEnvioRespuesta = true
            };

            return await _dinamicoSeguimientoService.GetReporteCasosEAPBExcelAsync(request);
        }

        [HttpGet("ReporteCasosEntidad/{fechaInicial}/{fechaFinal}/{idEntidad}")]
        public async Task<List<ReporteCasosEAPBDto>> ReporteCasosEntidad(DateTime fechaInicial, DateTime fechaFinal, int idEntidad)
        {
            var request = new ReporteCasosEntidadRequestDto
            {
                FechaInicial = fechaInicial,
                FechaFinal = fechaFinal,
                Entidad = idEntidad,
                AreaProcedencia = true,
                CategoriaAlerta = true,
                DepartamentoProcedencia = true,
                DireccionProcedencia = true,
                SubcategoriaAlerta = true,
                Respuesta = true,
                MunicipioProcedencia = true,
                DepartamentoActual = true,
                Observaciones = true,
                TipoIdentificacion = true,
                BarrioProcedencia = true,
                EAPB = true,
                NumeroIdentificacion = true,
                RegimenAfiliacion = true,
                EstadoNNA = true,
                IdCicloVida = 1,
                FechaEnvioRespuesta = true
            };

            return await _dinamicoSeguimientoService.GetReporteCasosEntidadAsync(request);
        }

        [HttpGet("ReporteCasosEntidadExcel/{fechaInicial}/{fechaFinal}/{idEntidad}")]
        public async Task<string> ReporteCasosEntidadExcel(DateTime fechaInicial, DateTime fechaFinal, int idEntidad)
        {
            var request = new ReporteCasosEntidadRequestDto
            {
                FechaInicial = fechaInicial,
                FechaFinal = fechaFinal,
                Entidad = idEntidad,
                AreaProcedencia = true,
                CategoriaAlerta = true,
                DepartamentoProcedencia = true,
                DireccionProcedencia = true,
                SubcategoriaAlerta = true,
                Respuesta = true,
                MunicipioProcedencia = true,
                DepartamentoActual = true,
                Observaciones = true,
                TipoIdentificacion = true,
                BarrioProcedencia = true,
                EAPB = true,
                NumeroIdentificacion = true,
                RegimenAfiliacion = true,
                EstadoNNA = true,
                IdCicloVida = 1,
                FechaEnvioRespuesta = true
            };

            return await _dinamicoSeguimientoService.GetReporteCasosEntidadExcelAsync(request);
        }
    }
}
