using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Mapster;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NNAController(INNAService service,INNARepo nNARepo, TablaParametricaService tablaParametrica) : ControllerBase
    {
        private INNARepo _nNARepo=nNARepo;
        private INNAService _nNAService = service;
        private readonly TablaParametricaService tablaParametricaService=tablaParametrica;


        /**
        * NNA
        */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var response = await _nNARepo.GetById(id);
            return Ok(response);
        }
        [HttpPost("Crear")]
        public async Task<ActionResult<RespuestaResponse<NNADto>>> AddAsync(NNADto dto)
        {
            return await _nNAService.AddAsync(dto);
        }

        [HttpPut("Actualizar")]
        public async Task<(bool, NNAs)> UpdateAsync(NNADto dto)
        {

            // Mapear NNADto a NNAs
            var entity = dto.Adapt<NNAs>();
            return await _nNARepo.UpdateAsync(entity);
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
    }
}
