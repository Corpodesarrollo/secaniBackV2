using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    [Route("[controller]")]
    //[Authorize]
    [ApiController]
    public class ParentescosController(ITPParentescos service) : Controller
    {
        private ITPParentescos _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TPParentescosDto>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _service.GetAllAsync();
            return Ok(entities);
        }
    }
}
