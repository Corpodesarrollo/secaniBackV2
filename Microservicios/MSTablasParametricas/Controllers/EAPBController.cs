using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [AllowAnonymous]
    public class EAPBController(IEAPBRepo service) : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<List<TPEAPBDto>>> GetEAPB(CancellationToken cancellationToken)
        {
            var result = await service.GetEAPB();
            return Ok(result);
        }

        [HttpGet("Search/{cadena}")]
        public async Task<ActionResult<List<TPEAPBDto>>> Search(string cadena, CancellationToken cancellationToken)
        {
            var result = await service.Search(cadena);
            return Ok(result);
        }

        [HttpGet("byCodigo/{codigo}")]
        public async Task<ActionResult<List<TPEAPBDto>>> GetByCodigo(string codigo, CancellationToken cancellationToken)
        {
            var result = await service.GetEAPBByCode(codigo);
            return Ok(result);
        }

        [HttpGet("byId/{id}")]
        public async Task<ActionResult<List<TPEAPBDto>>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await service.GetEAPBById(id);
            return Ok(result);
        }

        [HttpGet("byCodigoOrId/{value}")]
        public async Task<ActionResult<List<TPEAPBDto>>> GetByCodigoOrId(string value, CancellationToken cancellationToken)
        {
            var result = await service.GetEAPBByCodeOrId(value);

            // Si no encontró por ningún método
            if (result == null)
                return NotFound($"No se encontró ningún registro con el valor '{value}'.");

            return Ok(result);
        }

        [HttpGet("Entidades")]
        public async Task<ActionResult> Entidates(CancellationToken cancellationToken)
        {
            var result = await service.Entidates();
            return Ok(result);
        }
    }
}
