using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;


namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NNAController : ControllerBase
    {
        private INNARepo _nNARepo;

        public NNAController(INNARepo nNARepo)
        {
            _nNARepo= nNARepo;
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
            contactoNNA.Nombres= request.Nombres;
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

        [HttpPost("ActualizarNNASeguimiento")]
        public IActionResult ActualizarNNASeguimiento(NNASeguimientoRequest request)
        {
            _nNARepo.ActualizarNNASeguimiento(request);
            return Ok();
        }
    }
}
