using Core.Authorization;
using Core.Common;
using Core.DTOs;
using Core.DTOs.MSTablasParametricas;
using Core.Interfaces;
using Core.Interfaces.Services.MSTablasParametricas;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MSAuthentication.Api.Controllers
{
    public sealed class ContactoEntidadController(
        IContactoEntidadService service,
        IValidator<ContactoEntidadRequest> validator,
        IHistoricoTransaccionService historicoService) : BaseController
    {
        private const string NombreTablaHistorico = "ContactoEntidad";

        private readonly IContactoEntidadService _service = service;
        private readonly IValidator<ContactoEntidadRequest> _validator = validator;
        private readonly IHistoricoTransaccionService _historicoService = historicoService;

        // BUG-LZ-027: helper para registrar cada accion sobre ContactoEntidad en HistoricoTransaccion
        // (asi el historico real no se sobreescribe con la ultima actualizacion, como ocurria al
        // depender solo de DateUpdated en BaseEntity).
        private async Task RegistrarHistoricoAsync(string transaccion, object? registroAnterior, object? registroNuevo, CancellationToken cancellationToken)
        {
            try
            {
                var historico = new HistoricoTransaccion
                {
                    Id = Guid.NewGuid().ToString(),
                    NombreTabla = NombreTablaHistorico,
                    FechaTransaccion = DateTime.UtcNow,
                    Transaccion = transaccion,
                    UsuarioId = User?.Identity?.Name ?? "Sistema",
                    RegistroAnterior = registroAnterior != null ? JsonSerializer.Serialize(registroAnterior) : string.Empty,
                    RegistroNuevo = registroNuevo != null ? JsonSerializer.Serialize(registroNuevo) : string.Empty,
                    Comentario = string.Empty
                };
                if (!string.IsNullOrEmpty(historico.RegistroAnterior) && !string.IsNullOrEmpty(historico.RegistroNuevo))
                {
                    historico.Comentario = historico.ObtenerCamposModificados();
                }
                await _historicoService.GuardarHistoricoAsync(historico, cancellationToken);
            }
            catch
            {
                // El registro de historico no debe abortar la operacion principal.
            }
        }



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

        [HttpGet("Entidades/{idEntidad}")]
        [ProducesResponseType(typeof(List<ContactoEntidadResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetContactosEntidadesByEntidad(string idEntidad, CancellationToken cancellationToken)
        {
            var ContactoEntidads = await _service.Entidades(cancellationToken, idEntidad);
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
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
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
                return BadRequest(new { field = "email", message = "El correo ya existe" });
            }
            var (response, contactoEntidad) = await _service.AddAsync(request, cancellationToken);
            if (response)
            {
                await RegistrarHistoricoAsync("Adicionar", null, contactoEntidad, cancellationToken);
            }
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
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateContactoEntidad(long id, [FromBody] ContactoEntidadRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Validar email duplicado (otro contacto distinto al actual)
            var existingByEmail = await _service.GetByEmailAsync(request.Email, cancellationToken);
            if (existingByEmail != null && existingByEmail.Id != id)
            {
                return BadRequest(new { field = "email", message = "El correo ya existe" });
            }

            var contactoAntes = await _service.GetByIdAsync(id, cancellationToken);
            var (result, contacto) = await _service.UpdateAsync(id, request, cancellationToken);
            if (result)
            {
                await RegistrarHistoricoAsync("Actualizacion", contactoAntes, contacto, cancellationToken);
            }

            return CreatedAtAction(nameof(GetContactoEntidadById), new { Id = contacto.Id }, contacto);
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = PoliticasPermisos.RequiereCoordinadorAdmin)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<bool> DeleteContactoEntidad(long id, CancellationToken cancellationToken)
        {
            var contactoEntidad = await _service.GetByIdAsync(id, cancellationToken);

            if (contactoEntidad == null)
            {
                throw new Exception($"Contacto Entidad con identificar {id} not found");
            }

            var result = await _service.DeleteAsync(contactoEntidad, cancellationToken);
            if (result)
            {
                await RegistrarHistoricoAsync("Eliminar", contactoEntidad, null, cancellationToken);
            }
            return result;
        }

        // BUG-LZ-027: endpoint para que el frontend muestre el historico real (multiples registros)
        // del ContactoEntidad. Antes solo mostraba dateCreated/dateUpdated/dateDeleted de BaseEntity
        // y sobreescribia con cada update.
        [HttpGet("Historico/{id}")]
        [ProducesResponseType(typeof(IEnumerable<HistoricoTransaccionDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistoricoByContacto(long id, CancellationToken cancellationToken)
        {
            var historicos = await _historicoService.GetHistoricoByTablaAsync(NombreTablaHistorico, cancellationToken);
            if (historicos == null)
            {
                return Ok(Array.Empty<HistoricoTransaccionDTO>());
            }

            var filtrados = historicos
                .Where(h => RegistroContieneId(h.RegistroNuevo, id) || RegistroContieneId(h.RegistroAnterior, id))
                .OrderByDescending(h => h.FechaTransaccion)
                .ToList();
            return Ok(filtrados);
        }

        private static bool RegistroContieneId(string? json, long id)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Id", out var idProp) && idProp.TryGetInt64(out var jsonId))
                {
                    return jsonId == id;
                }
                // Fallback en caso de casing camel
                if (doc.RootElement.TryGetProperty("id", out var idProp2) && idProp2.TryGetInt64(out var jsonId2))
                {
                    return jsonId2 == id;
                }
            }
            catch { }
            return false;
        }
    }
}