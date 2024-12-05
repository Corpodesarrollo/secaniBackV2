using Core.Interfaces.Services.MSUsuariosyRoles;
using Microsoft.AspNetCore.Mvc;
namespace MSAuthentication.Api.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly Client _clienteServicio;

        public PersonaController(Client clienteServicio)
        {
            _clienteServicio = clienteServicio;
        }

        /// <summary>
        /// Retorna la identificación vigente de la persona
        /// </summary>
        /// <param name="tipoIdentificacion">Tipo de identificación (RC, TI, CC, etc.)</param>
        /// <param name="nroIdentificacion">Número de identificación</param>
        /// <param name="fechaExpedicion">Fecha de expedición (opcional)</param>
        /// <returns>Identificación vigente</returns>
        [HttpGet("GetIdVigente/{tipoIdentificacion}/{nroIdentificacion}")]
        public async Task<IActionResult> GetIdVigente(string tipoIdentificacion, string nroIdentificacion, [FromQuery] string fechaExpedicion = null)
        {
            var apiKey = "fa90576d-05e9-48ad-8bcf-30371984a699";

            try
            {
                var resultado = fechaExpedicion == null
                    ? await _clienteServicio.GetIdVigenteAsync(apiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdVigente2Async(apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);

                return Ok(resultado);
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, ex.Response);
            }
        }

        /// <summary>
        /// Retorna todas las identificaciones asociadas a una persona
        /// </summary>
        /// <param name="tipoIdentificacion">Tipo de identificación (RC, TI, CC, etc.)</param>
        /// <param name="nroIdentificacion">Número de identificación</param>
        /// <param name="fechaExpedicion">Fecha de expedición (opcional)</param>
        /// <returns>Lista de identificaciones asociadas</returns>
        [HttpGet("GetIdAll/{tipoIdentificacion}/{nroIdentificacion}")]
        public async Task<IActionResult> GetIdAll(string tipoIdentificacion, string nroIdentificacion, [FromQuery] string fechaExpedicion = null)
        {
            var apiKey = "fa90576d-05e9-48ad-8bcf-30371984a699";

            try
            {
                var resultado = fechaExpedicion == null
                    ? await _clienteServicio.GetIdAllAsync(apiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdAll2Async(apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);

                return Ok(resultado);
            }
            catch (ApiException ex)
            {
                return StatusCode(ex.StatusCode, ex.Response);
            }
        }
    }

}
