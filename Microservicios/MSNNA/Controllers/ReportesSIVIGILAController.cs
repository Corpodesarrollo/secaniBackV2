using Core.Authorization;
using Core.Common;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Web.MVCCore;

namespace MSNNA.Api.Controllers
{
    public class ReportesSIVIGILAController : BaseController
    {
        private readonly IReportesSIVIGILARepo _reportesSIVIGILARepo;

        public ReportesSIVIGILAController(IReportesSIVIGILARepo reportesSIVIGILARepo)
        {
            _reportesSIVIGILARepo = reportesSIVIGILARepo;
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
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<ActionResult> AddAsync(ReportesSIVIGILADto data)
        {
            var user = this.GetUser();
            var (success, response) = await _reportesSIVIGILARepo.AddAsync(data, user);
            if (!success)
                return BadRequest();

            return Ok(response);
        }

        [HttpPost("CrearSeguimiento")]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<ActionResult> CrearSeguimiento(SeguimientoDto data)
        {
            var user = this.GetUser();
            var result = await _reportesSIVIGILARepo.CrearSeguimiento(data, user);
            if (!result)
                return BadRequest();

            return Ok(result);
        }

        [HttpPut]
        [Authorize(Policy = PoliticasPermisos.RequiereAgenteOAdmin)]
        public async Task<ActionResult> UpdateAsync(ReportesSIVIGILADto entity)
        {
            var (success, response) = await _reportesSIVIGILARepo.UpdateAsync(entity);
            if (!success)
                return BadRequest();

            return Ok(response);
        }
    }
}
