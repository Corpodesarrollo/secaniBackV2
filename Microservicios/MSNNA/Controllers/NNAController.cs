using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NNAController : ControllerBase
    {
        private INNARepo _nNARepo;
        private readonly TablaParametricaService tablaParametricaService;

        public NNAController(INNARepo nNARepo, TablaParametricaService tablaParametrica)
        {
            _nNARepo = nNARepo;
            tablaParametricaService = tablaParametrica;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var response = await _nNARepo.GetById(id);
            return Ok(response);
        }

        [HttpPost("ContactoNNACrear")]
        public IActionResult ContactoNNACrear(ContactoNNARequest request)
        {
            var contactoNNA = new ContactoNNA();
            contactoNNA.Nombres = request.Nombres;
            contactoNNA.ParentescoId = request.ParentescoId;
            contactoNNA.Email = request.Email;
            contactoNNA.Telefonos = request.Telefonos;
            contactoNNA.TelefnosInactivos = request.TelefnosInactivos;

            var response = _nNARepo.CrearContactoNNA(contactoNNA);
            return Ok(response);
        }

        [HttpPut("ContactoNNAActualizar")]
        public IActionResult ContactoNNAActualizar(ContactoNNARequest request)
        {
            var contactoNNA = new ContactoNNA();
            contactoNNA.Nombres = request.Nombres;
            contactoNNA.ParentescoId = request.ParentescoId;
            contactoNNA.Email = request.Email;
            contactoNNA.Telefonos = request.Telefonos;
            contactoNNA.TelefnosInactivos = request.TelefnosInactivos;

            var response = _nNARepo.ActualizarContactoNNA(contactoNNA);
            return Ok(response);
        }

        [HttpGet("ContactoNNAGetById/{id}")]
        public IActionResult ContactoNNAGetById(long id)
        {

            var response = _nNARepo.ObtenerContactoPorId(id);
            return Ok(response.Datos);
        }

        [HttpGet("TpEstadosNNA")]
        public IActionResult TpEstadosNNA()
        {

            var response = _nNARepo.TpEstadosNNA();
            return Ok(response);
        }

        [HttpGet("VwAgentesAsignados")]
        public IActionResult VwAgentesAsignados()
        {

            var response = _nNARepo.VwAgentesAsignados();
            return Ok(response);
        }

        [HttpPost("ConsultarNNAFiltro")]
        public IActionResult ConsultarNNAFiltro(FiltroNNARequest request)
        {
            var response = _nNARepo.ConsultarNNAFiltro(request);
            return Ok(response.Datos);
        }

        [HttpGet("ConsultarNNAsByTipoIdNumeroId/{tipoIdentificacionId}/{numeroIdentificacion}")]
        public IActionResult ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion)
        {

            var response = _nNARepo.ConsultarNNAsByTipoIdNumeroId(tipoIdentificacionId, numeroIdentificacion);
            return Ok(response);
        }

        [HttpPost("ActualizarNNASeguimiento")]
        public IActionResult ActualizarNNASeguimiento(NNASeguimientoRequest request)
        {
            _nNARepo.ActualizarNNASeguimiento(request);
            return Ok();
        }



        /***
         * Migración de metodos al proyecto 
         * */
        [HttpGet("GetTpTipoId")]
        public IActionResult GetTpTipoId()
        {

            var response = _nNARepo.GetTpTipoId();
            return Ok(response);
        }

        [HttpGet("GetTPTipoIdentificacion")]
        public IActionResult GetTPTipoIdentificacion()
        {

            var response = _nNARepo.GetTPTipoIdentificacion();
            return Ok(response);
        }

        [HttpGet("GetTPRegimenAfiliacion")]
        public IActionResult GetTPRegimenAfiliacion()
        {

            var response = _nNARepo.GetTPRegimenAfiliacion();
            return Ok(response);
        }

        [HttpGet("GetTPParentesco")]
        public IActionResult GetTPParentesco()
        {

            var response = _nNARepo.GetTPParentesco();
            return Ok(response);
        }

        [HttpGet("GetTPPais")]
        public IActionResult GetTPPais()
        {

            var response = _nNARepo.GetTPPais();
            return Ok(response);
        }

        [HttpGet("GetTPDepartamento/{PaisId}")]
        public IActionResult GetTPDepartamento(int PaisId)
        {

            var response = _nNARepo.GetTPDepartamento(PaisId);
            return Ok(response);
        }

        [HttpGet("GetTPCiudad/{DepartamentoId}")]
        public IActionResult GetTPCiudad(int DepartamentoId)
        {

            var response = _nNARepo.GetTPCiudad(DepartamentoId);
            return Ok(response);
        }

        [HttpGet("GetTPOrigenReporte")]
        public IActionResult GetTPOrigenReporte()
        {

            var response = _nNARepo.GetTPOrigenReporte();
            return Ok(response);
        }

        [HttpGet("GetGrupoPoblacional")]
        public IActionResult GetGrupoPoblacional()
        {

            var response = _nNARepo.GetGrupoPoblacional();
            return Ok(response);
        }

        [HttpGet("GetTPEtnia")]
        public IActionResult GetTPEtnia()
        {

            var response = _nNARepo.GetTPEtnia();
            return Ok(response);
        }

        [HttpGet("GetTPEAPB")]
        public IActionResult GetTPEAPB()
        {

            var response = _nNARepo.GetTPEAPB();
            return Ok(response);
        }

        [HttpGet("GetTPEstadoIngresoEstrategia")]
        public IActionResult GetTPEstadoIngresoEstrategia()
        {

            var response = _nNARepo.GetTPEstadoIngresoEstrategia();
            return Ok(response);
        }

        [HttpGet("ConsultarNNAsById/{NNAId}")]
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

        [HttpGet("SolicitudSeguimientoCuidador/{NNAId}")]
        public async Task<IActionResult> SolicitudSeguimientoCuidador(long NNAId)
        {
            var response = await _nNARepo.SolicitudSeguimientoCuidador(NNAId, tablaParametricaService);
            return Ok(response);
        }
    }
}
