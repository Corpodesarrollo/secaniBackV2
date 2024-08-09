using Core.Modelos;
using Core.Request;
using Core.response;

namespace Core.Interfaces.Repositorios
{
    public interface IIntentoRepo
    {
        List<GetIntentoResponse> RepoIntentoUsuario(int UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        int RepoInsertarIntento(PostIntentoRequest request);
        int RepoIntentoActualizacionFecha(PutIntentoActualizacionFechaRequest request);
        int RepoIntentoActualizacionUsuario(PutIntentoActualizacionUsuarioRequest request);
        List<GetIntentoContactoAgrupadoResponse> RepoIntentoContactoAgrupado(int NNAId);
        List<GetContactoNNAIntentoResponse> RepoIntentosContactoNNA(int NNAId);
        public List<TPTipoFallaLLamada> RepoTipoFallas();
    }
}
