using Core.DTOs;
using Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MSAuthentication.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public sealed class ContactoEntidadController(IContactoEntidadService service, IValidator<ContactoEntidadRequest> validator) : ControllerBase
    {
        private readonly IContactoEntidadService _service = service;
        private readonly IValidator<ContactoEntidadRequest> _validator = validator;



        /// <summary>
        /// Gets all of the ContactoEntidads.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The collection of ContactoEntidad    .</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ContactoEntidadResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContactosEntidades(CancellationToken cancellationToken)
        {
            var ContactoEntidads = await _service.GetAllAsync(cancellationToken);
            return Ok(ContactoEntidads);
        }

        /// <summary>
        /// Gets the ContactoEntidad with the specified identifier, if it exists.
        /// </summary>
        /// <param name="Id">The ContactoEntidad identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The ContactoEntidad with the specified identifier, if it exists.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ContactoEntidadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetContactoEntidadById(long id, CancellationToken cancellationToken)
        {
            var contactoEntidad = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(contactoEntidad);
        }

        /// <summary>
        /// Creates a new ContactoEntidad based on the specified request.
        /// </summary>
        /// <param name="request">The create ContactoEntidad request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly created ContactoEntidad.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ContactoEntidadResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddContactoEntidad([FromBody] ContactoEntidadRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var contactoEntidadResponse = await _service.GetByEmailAsync(request.Email, cancellationToken);
            if (contactoEntidadResponse != null)
            {
                throw new Exception($"Contacto Entidad con email {request.Email} already exists");
            }
            var (response, contactoEntidad) = await _service.AddAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetContactoEntidadById), new { Id = contactoEntidad.Id }, contactoEntidad);
        }

        /// <summary>
        /// Updates the ContactoEntidad with the specified identifier based on the specified request, if it exists.
        /// </summary>
        /// <param name="Id">The ContactoEntidad identifier.</param>
        /// <param name="request">The update ContactoEntidad request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The updated ContactoEntidad</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContactoEntidad(long id, [FromBody] ContactoEntidadRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var (result, contacto) = await _service.UpdateAsync(id, request, cancellationToken);

            return CreatedAtAction(nameof(GetContactoEntidadById), new { Id = contacto.Id }, contacto);
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> DeleteContactoEntidad(long id, CancellationToken cancellationToken)
        {
            var contactoEntidad = await _service.GetByIdAsync(id, cancellationToken);

            if (contactoEntidad == null)
            {
                throw new Exception($"Contacto Entidad con identificar {id} not found");
            }

            return await _service.DeleteAsync(contactoEntidad, cancellationToken);
        }
    }
}