using Core.Common;
using Core.DTOs.Reportes;
using Core.Interfaces.Services.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSNNA.Api.Controllers
{
    [AllowAnonymous]
    public class ReporteDinamicoEAPBController(IReporteDinamicoEAPBService service) : BaseController
    {
        private readonly IReporteDinamicoEAPBService _service = service;

        [HttpGet]
        public async Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime FechaInicial, DateTime FechaFinal)
        {
            return await _service.GetReporteDinamicoEAPBAsync(FechaInicial, FechaFinal, cancellationToken: default);
        }
    }
}
