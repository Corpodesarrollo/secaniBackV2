using Core.Interfaces.MSTablasParametricas;
using Microsoft.AspNetCore.Mvc;

namespace MSTablasParametricas.Api.Controllers.Common
{
    [Route("[controller]")]
    [ApiController]
    public class GenericController<T1, T2> : ControllerBase where T1 : class where T2 : class
    {
        private readonly IGenericService<T1, T2> _service;

        public GenericController(IGenericService<T1, T2> service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<T2>> GetById(int id, CancellationToken cancellationToken)
        {
            var entity = await _service.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<T2>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _service.GetAllAsync(cancellationToken);
            return Ok(entities);
        }

        [HttpGet("NotDeleted")]
        public async Task<ActionResult<IEnumerable<T2>>> GetAllNotDeleted(CancellationToken cancellationToken)
        {
            var entities = await _service.GetAllNotDeletedAsync(cancellationToken);
            return Ok(entities);
        }


        [HttpPost]
        public async Task<ActionResult<T2>> Add([FromBody] T1 entity, CancellationToken cancellationToken)
        {
            // Obtener la propiedad 'Id'
            var idProp = entity.GetType().GetProperty("Id");

            // Si existe, establecerla en null (solo si es nullable o referencia)
            if (idProp != null && idProp.CanWrite)
            {
                if (idProp.PropertyType == typeof(int) || idProp.PropertyType == typeof(long))
                {
                    idProp.SetValue(entity, Activator.CreateInstance(idProp.PropertyType));
                }
                else
                {
                    idProp.SetValue(entity, null);
                }
            }

            var (success, createdEntity) = await _service.AddAsync(entity, cancellationToken);

            if (success)
            {
                var idValue = createdEntity.GetType().GetProperty("Id")?.GetValue(createdEntity);
                return CreatedAtAction(nameof(GetById), new { id = idValue }, createdEntity);
            }

            return BadRequest();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] T1 entity, CancellationToken cancellationToken)
        {
            if (!id.Equals(entity.GetType().GetProperty("Id").GetValue(entity)))
            {
                return BadRequest();
            }

            var (success, updatedEntity) = await _service.UpdateAsync(entity, cancellationToken);
            if (success)
            {
                return Ok(updatedEntity);
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(id, cancellationToken);
            return Ok(result); // true si se marcó como eliminada, false si no tiene IsDeleted o falló
        }
    }
}