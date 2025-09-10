using Core.Common;
using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers
{
    public class ParentescosController(ITPParentescos service) : BaseController
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
