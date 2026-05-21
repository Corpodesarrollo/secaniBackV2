using Core.Common;
using Core.DTOs;
using Core.DTOs.MSTablasParametricas;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.MSTablasParametricas;
using Core.Modelos;
using Core.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace Api.Controllers
{
    public class ContactoNNAsController(
        IContactoNNARepo service,
        IHistoricoTransaccionService historicoService) : BaseController
    {
        private IContactoNNARepo _service = service;
        private readonly IHistoricoTransaccionService _historicoService = historicoService;
        private const string NombreTablaHistorico = "ContactoNNA";

        // BUG-LZ-018: registrar Add/Update de ContactoNNA en HistoricoTransaccion para que
        // aparezca en el historico del NNA. Paridad con ContactoEntidad (BUG-LZ-027).
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
                // historico best-effort: no debe abortar operacion principal
            }
        }

        // Consulta
        [HttpGet("Obtener/{id}")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAGetById(long id)
        {
            var response = await _service.Obtener(id);
            return Ok(response);
        }

        [HttpGet("ObtenerByNNAId/{nNAId}")]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAGetByNNAId(long nNAId)
        {
            var response = await _service.ObtenerByNNAId(nNAId);
            return Ok(response);
        }

        // Operaciones
        [HttpPost]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNACreate(ContactoNNADto dto, CancellationToken cancellationToken)
        {
            var response = await _service.CrearContactoNNA(dto);
            if (response.Estado)
            {
                await RegistrarHistoricoAsync("Adicionar", null, response.Datos, cancellationToken);
            }
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult<RespuestaResponse<ContactoNNADto>>> ContactoNNAUpdate(ContactoNNADto dto, CancellationToken cancellationToken)
        {
            var antes = await _service.Obtener(dto.Id);
            var response = await _service.ContactoNNAActualizar(dto);
            if (response.Estado)
            {
                await RegistrarHistoricoAsync("Actualizacion", antes?.Datos, response.Datos, cancellationToken);
            }
            return Ok(response);
        }

        // BUG-LZ-018: endpoint para que detalle-nna muestre actividad de cambios sobre los
        // contactos de un NNA especifico. Filtra HistoricoTransaccion por NombreTabla=ContactoNNA
        // y por NNAId dentro del JSON serializado (registroAnterior o registroNuevo).
        [HttpGet("Historico/{nnaId}")]
        [ProducesResponseType(typeof(IEnumerable<HistoricoTransaccionDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistoricoByNNA(long nnaId, CancellationToken cancellationToken)
        {
            var historicos = await _historicoService.GetHistoricoByTablaAsync(NombreTablaHistorico, cancellationToken);
            if (historicos == null)
            {
                return Ok(Array.Empty<HistoricoTransaccionDTO>());
            }

            var filtrados = historicos
                .Where(h => RegistroContieneNNAId(h.RegistroNuevo, nnaId) || RegistroContieneNNAId(h.RegistroAnterior, nnaId))
                .OrderByDescending(h => h.FechaTransaccion)
                .ToList();
            return Ok(filtrados);
        }

        private static bool RegistroContieneNNAId(string? json, long nnaId)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("NNAId", out var prop) && prop.TryGetInt64(out var jsonId))
                {
                    return jsonId == nnaId;
                }
                if (doc.RootElement.TryGetProperty("nNAId", out var prop2) && prop2.TryGetInt64(out var jsonId2))
                {
                    return jsonId2 == nnaId;
                }
                if (doc.RootElement.TryGetProperty("nnaId", out var prop3) && prop3.TryGetInt64(out var jsonId3))
                {
                    return jsonId3 == nnaId;
                }
            }
            catch { }
            return false;
        }
    }
}
