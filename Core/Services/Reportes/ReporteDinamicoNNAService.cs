using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.Reportes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Reportes
{
    public class ReporteDinamicoNNAService(IReporteDinamicoNNARepository repository) : IReporteDinamicoNNAService
    {
        private readonly IReporteDinamicoNNARepository _repository = repository;
        public Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            return _repository.GetReporteDinamicoNNAAsync(fechaInicio, fechaFin, cancellationToken);
        }
    }
}
