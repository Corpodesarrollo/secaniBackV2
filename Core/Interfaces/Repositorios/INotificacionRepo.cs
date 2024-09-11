using Core.Request;
using Core.response;
using Core.Response;

namespace Core.Interfaces.Repositorios
{
    public interface INotificacionRepo
    {
        public List<GetNotificacionResponse> GetNotificacionUsuario(string AgenteDestinoId);
        public int GetNumeroNotificacionUsuario(string AgenteDestinoId);
        public string GenerarOficioNotificacion(OficioNotificacionRequest request);
        public void EliminarNotificacion(EliminarNotificacionRequest request);
        public Task<string> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request);
        public VerOficioNotificacionResponse VerOficioNotificacion(VerOficioNotificacionRequest request);

        List<GetNotificacionesEntidadResponse> RepoNotificacionEntidadCasos(string entidadId, int alertaSeguimientoId, int nnaId);
        List<GetListaCasosResponse> RepoListaCasosNotificacion(int eapbId, int epsId);
    }
}
