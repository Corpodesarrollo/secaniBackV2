using Core.Request;

namespace Core.Interfaces
{
    public interface IHelperRepo
    {
        Task<UploadFileRequest?> EvidenciaDiagnostico(long id);
        Task<UploadFileRequest?> EvidenciaParentesco(long id);
    }
}
