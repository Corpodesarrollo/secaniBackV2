using Core.Common;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class PersonaController : BaseController
    {
        private readonly IPersonaService _personaService;

        public PersonaController(IPersonaService personaService)
        {
            _personaService = personaService;
        }

        [HttpGet("GetIdVigente/{tipoIdentificacion}/{nroIdentificacion}")]
        public async Task<IActionResult> GetIdVigente(string tipoIdentificacion, string nroIdentificacion, [FromQuery] string fechaExpedicion = null)
        {
            try
            {
                var resultado = await _personaService.GetIdVigenteAsync(tipoIdentificacion, nroIdentificacion, fechaExpedicion);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetIdAll/{tipoIdentificacion}/{nroIdentificacion}")]
        public async Task<IActionResult> GetIdAll(string tipoIdentificacion, string nroIdentificacion, [FromQuery] string fechaExpedicion = null)
        {
            try
            {
                var resultado = await _personaService.GetIdAllAsync(tipoIdentificacion, nroIdentificacion, fechaExpedicion);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetTipoIdentificacionVigente/{nroIdentificacion}")]
        public async Task<IActionResult> GetTipoIdentificacionVigente(string nroIdentificacion)
        {
            try
            {
                var resultado = await _personaService.GetTipoIdentificacionVigenteAsync(nroIdentificacion);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
