using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactoNNAsController(IContactoNNAService service) : ControllerBase
    {
        private IContactoNNAService _service = service;


         // Consulta
        [HttpGet("Obtener/{id}")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAGetById(long id)
        {
            var response = await _service.Obtener(id);
            return Ok(response);
        }

        [HttpGet("ObtenerByNNAId/{nNAId}")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAGetByNNAId(long nNAId)
        {
            var response = await _service.ObtenerByNNAId(nNAId);
            return Ok(response);
        }


        //Operacion
        [HttpPost("Crear")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNACrear(ContactoNNADto dto)
        {
            var response = await _service.CrearContactoNNA(dto);
            return Ok(response);
        }

        [HttpPut("Actualizar")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAActualizar(ContactoNNADto dto)
        {
            var response = await _service.ContactoNNAActualizar(dto);
            return Ok(response);
        }



    }
}
