using Core.Common;
using Core.Modelos;
using Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MSNNA.Api.Controllers
{
    // BUG-LZ-089: contactos adicionales del cuidador en Mi perfil (telefonos + correos).
    // Antes el componente solo guardaba en memoria con un TODO -> al volver no se veia nada.
    public class ContactoCuidadorController(ApplicationDbContext context) : BaseController
    {
        public sealed class TelefonoDto
        {
            public string Numero { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
        }

        public sealed class CorreoDto
        {
            public string Correo { get; set; } = string.Empty;
        }

        public sealed class GuardarRequest
        {
            public string UserId { get; set; } = string.Empty;
            public List<TelefonoDto> Telefonos { get; set; } = new();
            public List<CorreoDto> Correos { get; set; } = new();
        }

        public sealed class ContactosResponse
        {
            public List<TelefonoDto> Telefonos { get; set; } = new();
            public List<CorreoDto> Correos { get; set; } = new();
        }

        [HttpGet("ContactosAdicionales/{userId}")]
        public async Task<ActionResult<ContactosResponse>> Get(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId es obligatorio.");

            var registros = await context.ContactosAdicionalesCuidador
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var respuesta = new ContactosResponse
            {
                Telefonos = registros.Where(c => c.Tipo == "telefono")
                    .Select(c => new TelefonoDto { Numero = c.Valor, Descripcion = c.Descripcion })
                    .ToList(),
                Correos = registros.Where(c => c.Tipo == "correo")
                    .Select(c => new CorreoDto { Correo = c.Valor })
                    .ToList()
            };

            return Ok(respuesta);
        }

        [HttpPost("ContactosAdicionales/Guardar")]
        public async Task<ActionResult> Guardar([FromBody] GuardarRequest req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.UserId))
                return BadRequest("UserId es obligatorio.");

            // Estrategia: reemplazo total. Soft-delete los actuales del cuidador y luego insert
            // los nuevos. Mantiene historico (IsDeleted=1) por si hace falta auditoria.
            var actuales = await context.ContactosAdicionalesCuidador
                .Where(c => c.UserId == req.UserId && !c.IsDeleted)
                .ToListAsync();

            foreach (var a in actuales)
            {
                a.IsDeleted = true;
                a.DateDeleted = DateTime.UtcNow;
                a.DeletedByUserId = req.UserId;
            }

            foreach (var t in req.Telefonos ?? new())
            {
                if (string.IsNullOrWhiteSpace(t.Numero)) continue;
                context.ContactosAdicionalesCuidador.Add(new ContactoAdicionalCuidador
                {
                    UserId = req.UserId,
                    Tipo = "telefono",
                    Valor = t.Numero.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(t.Descripcion) ? null : t.Descripcion.Trim(),
                    CreatedByUserId = req.UserId,
                    DateCreated = DateTime.UtcNow
                });
            }

            foreach (var c in req.Correos ?? new())
            {
                if (string.IsNullOrWhiteSpace(c.Correo)) continue;
                context.ContactosAdicionalesCuidador.Add(new ContactoAdicionalCuidador
                {
                    UserId = req.UserId,
                    Tipo = "correo",
                    Valor = c.Correo.Trim(),
                    CreatedByUserId = req.UserId,
                    DateCreated = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            return Ok(new { estado = true, descripcion = "Datos adicionales guardados." });
        }
    }
}
