using Core.Common;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISPRO.TRV.Web.MVCCore;
using System.Text.Json.Serialization;

namespace MSNNA.Api.Controllers
{
    public class ReportesSIVIGILAController : BaseController
    {
        private readonly IReportesSIVIGILARepo _reportesSIVIGILARepo;
        private readonly ApplicationDbContext _context;

        public ReportesSIVIGILAController(IReportesSIVIGILARepo reportesSIVIGILARepo, ApplicationDbContext context)
        {
            _reportesSIVIGILARepo = reportesSIVIGILARepo;
            _context = context;
        }

        // BUG-LZ-76: shape compatible con persona de Maestro SISPRO (snake_case) para que el
        // frontend pueda usar el fallback sin reescribir el componente Crear NNA.
        public sealed class PersonaSivigilaDto
        {
            [JsonPropertyName("primer_nombre")]
            public string? PrimerNombre { get; set; }
            [JsonPropertyName("segundo_nombre")]
            public string? SegundoNombre { get; set; }
            [JsonPropertyName("primer_apellido")]
            public string? PrimerApellido { get; set; }
            [JsonPropertyName("segundo_apellido")]
            public string? SegundoApellido { get; set; }
            [JsonPropertyName("fecha_nacimiento")]
            public DateTime? FechaNacimiento { get; set; }
            [JsonPropertyName("sexo")]
            public string? Sexo { get; set; }
            [JsonPropertyName("esFallecido")]
            public bool EsFallecido { get; set; }
        }

        // BUG-LZ-76: NNAs en cargue SIVIGILA pero no en Maestro Personas SISPRO eran rechazados
        // por el flujo de Crear NNA. Este endpoint sirve de fallback: si Maestro no responde, el
        // frontend consulta ReportesSIVIGILA local y rellena el formulario con los datos del
        // cargue masivo (siempre que NumeroIdentificacion + TipoIdentificacionId coincidan).
        [HttpGet("PersonaByDocumento/{tipo}/{numero}")]
        public async Task<ActionResult> PersonaByDocumento(string tipo, string numero)
        {
            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(numero))
                return Ok((object?)null);

            var registro = await _context.Set<Core.Modelos.ReportesSIVIGILA>()
                .Where(r => r.TipoIdentificacionId == tipo
                            && r.NumeroIdentificacion == numero
                            && !r.IsDeleted)
                .OrderByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            if (registro == null)
                return Ok((object?)null);

            var dto = new PersonaSivigilaDto
            {
                PrimerNombre = registro.PrimerNombre,
                SegundoNombre = registro.SegundoNombre,
                PrimerApellido = registro.PrimerApellido,
                SegundoApellido = registro.SegundoApellido,
                FechaNacimiento = registro.FechaNacimiento,
                Sexo = registro.SexoId,
                EsFallecido = false
            };
            return Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _reportesSIVIGILARepo.GetAll(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(long id)
        {
            var result = await _reportesSIVIGILARepo.GetById(id);
            return Ok(result);
        }

        [HttpGet("EvidenciaDiagnostico/{id}")]
        public async Task<ActionResult> EvidenciaDiagnostico(long id)
        {
            var result = await _reportesSIVIGILARepo.EvidenciaDiagnostico(id);
            return Ok(result);
        }

        [HttpGet("EvidenciaParentesco/{id}")]
        public async Task<ActionResult> EvidenciaParentesco(long id)
        {
            var result = await _reportesSIVIGILARepo.EvidenciaParentesco(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddAsync(ReportesSIVIGILADto data)
        {
            var user = this.GetUser();
            var (success, response) = await _reportesSIVIGILARepo.AddAsync(data, user);
            if (!success)
                return BadRequest();

            return Ok(response);
        }

        [HttpPost("CrearSeguimiento")]
        public async Task<ActionResult> CrearSeguimiento(SeguimientoDto data)
        {
            var user = this.GetUser();
            // BUG-LZ-040: antes retornaba 400 sin mensaje cuando el NNA no existia en NNAs (caso comun:
            // cuidador busca persona existente en SISPRO pero aun no creada como NNA en SECANI).
            // Frontend mostraba "Http failure response ... 400 Bad Request" sin contexto util.
            // Ahora delegamos al repo que retorna (bool, string?) con motivo, y devolvemos el mensaje.
            var (success, message) = await _reportesSIVIGILARepo.CrearSeguimientoDetallado(data, user);
            if (!success)
                return BadRequest(new { message });

            return Ok(new { success });
        }

        [HttpPut]
        public async Task<ActionResult> UpdateAsync(ReportesSIVIGILADto entity)
        {
            var (success, response) = await _reportesSIVIGILARepo.UpdateAsync(entity);
            if (!success)
                return BadRequest();

            return Ok(response);
        }
    }
}
