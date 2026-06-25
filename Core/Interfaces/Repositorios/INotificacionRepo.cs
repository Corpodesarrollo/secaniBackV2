using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using System.Net.Mail;

namespace Core.Interfaces.Repositorios
{
    public interface INotificacionRepo
    {
        public Task<List<GetNotificacionResponse>> GetNotificacionUsuario(string AgenteDestinoId);
        public Task<int> GetNumeroNotificacionUsuario(string AgenteDestinoId);
        public void EliminarNotificacion(EliminarNotificacionRequest request);
        List<GetNotificacionesEntidadResponse> RepoNotificacionEntidadCasos(long entidadId, int alertaSeguimientoId, int nnaId);
        List<GetListaCasosResponse> RepoListaCasosNotificacion(string eapbId, int epsId);
        public Task<List<NotificacionResponse>> GetNotificacionAlerta(long AlertaId);
        public Task<List<NotificacionResponse>> GetNotificacionSeguimiento(long SeguimientoId);
        Task<RespuestaResponse<bool>> NotificacionRespuesta(NotificacionRespuestaDto data);

        public Task<string> PlantillaCorreo(string[] Para, string[] ConCopia, string Asunto, string Body, string[] Adjuntos, Attachment AdjuntoPdf);
        Task<string> NotificacionReporteSivigila(long idReporteSivigila, string entidadId, string userId);
        Task<string> NotificacionSolicitudSeguimiento(string cuidadorId, long nnaId, string agenteSeguimientoId, string userId);

        Task RevisarYEnviarNotificaciones();
        Task EnviarNotificacionAsignacionCoordinadores(string[] para, List<UsuarioAsignado> asignados, List<UsuarioAsignado> reagendados, List<UsuarioAsignado> reasignados);
        Task EnviarNotificacionAsignacionAgentes(UserDto[] agentes, List<UsuarioAsignado> asignados);
        Task<RespuestaResponse<long>> GenerarOficioNotificacion(OficioNotificacionRequest request);
        Task<RespuestaResponse<string>> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request);
        Task<RespuestaResponse<OficioNotificacionRequest>> VerOficioNotificacion(long id);
        Task<RespuestaResponse<bool>> ValidarNotificacion(int id);
        Task<bool> SetNotificacion(GetNotificacionResponse data);
    }
}
