using Core.Authorization;
using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Web.MVCCore;


namespace Api.Controllers
{
    public class NNAController(INNAService service, INNARepo nNARepo, TablaParametricaService tablaParametrica) : BaseController
    {
        private INNARepo _nNARepo = nNARepo;
        private INNAService _nNAService = service;
        private readonly TablaParametricaService tablaParametricaService = tablaParametrica;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var response = await _nNARepo.GetById(id);
            return Ok(response);
        }

        [HttpPost("Crear")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<ActionResult<RespuestaResponse<NNADto>>> AddAsync(NNADto dto)
        {
            var user = this.GetUser();
            return await _nNAService.AddAsync(dto, user);
        }

        // HU RQ07-HU08 (EAPB) + RQ09-HU10 (ET): listar NNAs sin reporte SIVIGILA
        // eapbId: filtra EAPB del usuario. municipioId: filtra municipio exacto. departamentoId: filtra prefix codigo DANE (ET jurisdiccion)
        [HttpGet("PendientesSivigila")]
        public async Task<IActionResult> PendientesSivigila([FromQuery] int? eapbId, [FromQuery] string? municipioId, [FromQuery] string? departamentoId)
        {
            var result = await _nNARepo.GetPendientesSivigila(eapbId, municipioId, departamentoId);
            return Ok(result);
        }

        [HttpPut("Actualizar")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<(bool, NNAs?)> UpdateAsync(NNADto dto)
        {
            var user = this.GetUser();
            return await _nNAService.UpdateAsync(dto, user);
        }

        [HttpPost("ConsultarNNAFiltro")]
        [ProducesResponseType(typeof(RespuestaResponse<FiltroNNADto>), StatusCodes.Status200OK)]
        public IActionResult ConsultarNNAFiltro(FiltroNNARequest request)
        {
            var response = _nNARepo.ConsultarNNAFiltro(request);
            return Ok(response.Datos);
        }

        [HttpGet("ConsultarNNAsByTipoIdNumeroId/{tipoIdentificacionId}/{numeroIdentificacion}")]
        [ProducesResponseType(typeof(NNAResponse), StatusCodes.Status200OK)]
        public IActionResult ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {

            var response = _nNARepo.ConsultarNNAsByTipoIdNumeroId(tipoIdentificacionId, numeroIdentificacion);
            return Ok(response);
        }

        [HttpGet("ConsultarNNAsById/{NNAId}")]
        [ProducesResponseType(typeof(NNAResponse), StatusCodes.Status200OK)]
        public IActionResult ConsultarNNAsById(long NNAId)
        {

            var response = _nNARepo.ConsultarNNAsById(NNAId);
            return Ok(response);
        }

        [HttpGet("DatosBasicosNNAById/{NNAId}")]
        public async Task<IActionResult> ConsultarDatosBasicosNNAById(long NNAId)
        {

            var response = await _nNARepo.ConsultarDatosBasicosNNAById(NNAId, tablaParametricaService);

            return Ok(response);
        }

        /**
        * Muestra los agentes activos seguimiento
        */
        [HttpGet("VwAgentesAsignados")]
        [ProducesResponseType(typeof(List<VwAgentesAsignados>), StatusCodes.Status200OK)]
        public IActionResult VwAgentesAsignados()
        {

            var response = _nNARepo.VwAgentesAsignados();
            return Ok(response);
        }

        /**
        * Seguimiento
        */
        [HttpPost("ActualizarNNASeguimiento")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public IActionResult ActualizarNNASeguimiento(NNASeguimientoRequest request)
        {
            _nNARepo.ActualizarNNASeguimiento(request);
            return Ok();
        }

        [HttpGet("SolicitudSeguimientoCuidador/{NNAId}")]
        public async Task<IActionResult> SolicitudSeguimientoCuidador(long NNAId)
        {
            var response = await _nNARepo.SolicitudSeguimientoCuidador(NNAId, tablaParametricaService);
            return Ok(response);
        }

        [HttpPost("DepuracionProtocolo")]
        [ProducesResponseType(typeof(DepuracionProtocoloResponse), StatusCodes.Status200OK)]
        public IActionResult DepuracionProtocolo(List<DepuracionProtocoloRequest> request)
        {
            var response = _nNARepo.DepuracionProtocolo(request);
            return Ok(response);
        }

        [HttpPost("SetResidenciaDiagnosticoTratamiento")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {
            _nNARepo.SetResidenciaDiagnosticoTratamiento(request);
        }

        [HttpPost("SetDiagnosticoTratamiento")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            _nNARepo.SetDiagnosticoTratamiento(request);
        }

        [HttpPost("SetDificultadesProceso")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public void SetDificultadesProceso(DificultadesProcesoRequest request)
        {
            _nNARepo.SetDificultadesProceso(request);
        }

        [HttpPost("SetAdherenciaProceso")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request)
        {
            _nNARepo.SetAdherenciaProceso(request);
        }

        [HttpPost("CasosAbiertos")]
        public IActionResult ConsultaCasosAbiertos(CasosAbiertosRequest request)
        {
            var response = _nNARepo.ConsultaCasosAbiertos(request);
            return Ok(response);
        }

        [HttpPost("AsignacionManual")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        public void AsignacionManual(AsignacionManualRequest request)
        {
            _nNARepo.AsignacionManual(request);
        }

        [HttpPost("CargarArchivoNNA")]
        public async Task<IActionResult> CargarArchivoNNA(IFormFile file)
        {
            var response = await _nNARepo.CargarArchivoNNA(file);
            return Ok(response);
        }
    }
}