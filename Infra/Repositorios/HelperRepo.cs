using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Request;

namespace Infra.Repositorios
{
    public class HelperRepo(IReportesSIVIGILARepo _reportesSIVIGILARepo) : IHelperRepo
    {
        public async Task<UploadFileRequest?> EvidenciaDiagnostico(long id)
        {
            var evidenciaDiagnostico = await _reportesSIVIGILARepo.EvidenciaDiagnostico(id);
            return evidenciaDiagnostico;
        }

        public async Task<UploadFileRequest?> EvidenciaParentesco(long id)
        {
            var evidenciaParentesco = await _reportesSIVIGILARepo.EvidenciaParentesco(id);
            return evidenciaParentesco;
        }
    }
}
