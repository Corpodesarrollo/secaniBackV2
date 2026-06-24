using Core.DTOs.Reportes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Services.Reportes
{
    public interface IReporteDinamicoEAPBService
    {
        Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime fechaInicio, DateTime fechaFin, int? eapbId, string? departamentoId, CancellationToken cancellationToken);
    }
}
