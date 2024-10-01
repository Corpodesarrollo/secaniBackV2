using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NombresTablaParametricaController : ControllerBase
    {
        private readonly INombreTablaParametricaService _nombreTablaParametricaService;

        public NombresTablaParametricaController(INombreTablaParametricaService nombreTablaParametricaService)
        {
            _nombreTablaParametricaService = nombreTablaParametricaService;
        }

        // GET: api/TablaParametrica
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TablaParametricaDTO>>> GetAll()
        {
            var result = await _nombreTablaParametricaService.GetAllAsync();
            return Ok(result);
        }

        // GET: api/TablaParametrica/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TablaParametricaDTO>> GetById(string id)
        {
            var result = await _nombreTablaParametricaService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // GET: api/TablaParametrica/padre/{idPadre}
        [HttpGet("padre/{idPadre}")]
        public async Task<ActionResult<IEnumerable<TablaParametricaDTO>>> GetTablasByPadre(string idPadre)
        {
            var result = await _nombreTablaParametricaService.GetTablasByPadreAsync(idPadre);
            return Ok(result);
        }

        // GET: api/TablaParametrica/fuente/{idFuente}
        [HttpGet("fuente/{idFuente}")]
        public async Task<ActionResult<IEnumerable<TablaParametricaDTO>>> GetTablasByFuente(int idFuente)
        {
            var result = await _nombreTablaParametricaService.GetTablasByFuenteAsync(idFuente);
            return Ok(result);
        }

        // GET: api/TablaParametrica/fuente-sispro
        [HttpGet("fuente-sispro")]
        public async Task<ActionResult<IEnumerable<TablaParametricaDTO>>> GetTablasSISPRO()
        {
            var result = await _nombreTablaParametricaService.GetTablasByFuenteAsync(0); // SISPRO = 0
            return Ok(result);
        }

        // GET: api/TablaParametrica/fuente-local
        [HttpGet("fuente-local")]
        public async Task<ActionResult<IEnumerable<TablaParametricaDTO>>> GetTablasLocal()
        {
            var result = await _nombreTablaParametricaService.GetTablasByFuenteAsync(1); // Local = 1
            return Ok(result);
        }

        // POST: api/TablaParametrica
        [HttpPost]
        public async Task<ActionResult<TablaParametricaDTO>> Add([FromBody] TablaParametricaDTO tablaDTO)
        {
            if (tablaDTO == null)
                return BadRequest();

            var result = await _nombreTablaParametricaService.AddAsync(tablaDTO);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT: api/TablaParametrica/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] TablaParametricaDTO tablaDTO)
        {
            if (tablaDTO == null || tablaDTO.Id != id)
                return BadRequest();

            await _nombreTablaParametricaService.UpdateAsync(tablaDTO);
            return NoContent();
        }

        // DELETE: api/TablaParametrica/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existingItem = await _nombreTablaParametricaService.GetByIdAsync(id);
            if (existingItem == null)
                return NotFound();

            await _nombreTablaParametricaService.RemoveAsync(id);
            return NoContent();
        }
    }
}