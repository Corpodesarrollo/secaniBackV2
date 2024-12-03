using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Response;
using Microsoft.AspNetCore.Mvc;


namespace Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class ContactoNNAsController(IContactoNNARepo service) : ControllerBase
    {
        private IContactoNNARepo _service = service;

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

        // Operaciones
        [HttpPost]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNACreate(ContactoNNADto dto)
        {
            var response = await _service.CrearContactoNNA(dto);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAUpdate(ContactoNNADto dto)
        {
            var response = await _service.ContactoNNAActualizar(dto);
            return Ok(response);
        }
    }
}
