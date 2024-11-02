using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using MSTablasParametricas.Api.Controllers.Common;

namespace MSTablasParametricas.Api.Controllers
{

    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class FestivosController : GenericController<TPFestivos, FestivoDTO>
    {
        private readonly IFestivoService _service;
        public FestivosController(IFestivoService service) : base(service)
        {
            _service = service;
        }

        [HttpGet("EsFestivo/{date}")]
        public async Task<ActionResult<(bool, FestivoDTO)>> EsFestivo(DateOnly date, CancellationToken cancellationToken)
        {
            var (success, entity) = await _service.EsFestivoAsync(date, cancellationToken);
            if (entity == null)
            {
                return (false, null);
            }
            return (true, entity);
        }

        [HttpGet("FestivosByAno/{ano}")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> FestivosByAno(int ano, CancellationToken cancellationToken)
        {
            var entities = await _service.GetFestivosByAnoAsync(ano, cancellationToken);
            return Ok(entities);
        }

        [HttpGet("FestivosByAnoMes/{ano}/{mes}")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> FestivosByAnoMes(int ano, int mes, CancellationToken cancellationToken)
        {
            var entities = await _service.GetFestivosByAnoAndMesAsync(ano, mes, cancellationToken);
            return Ok(entities);
        }

        [HttpGet("GetFestivoByDate/{date}")]
        public async Task<ActionResult<IEnumerable<FestivoDTO>>> GetFestivoByDate(DateOnly date, CancellationToken cancellationToken)
        {
            var entity = await _service.GetFestivoByDateAsync(date, cancellationToken);
            return Ok(entity);
        }
    }
}
