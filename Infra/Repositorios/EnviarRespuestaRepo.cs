using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;
using static Core.Common.Estructuras;

namespace Infra.Repositorios
{
    public class EnviarRespuestaRepo(ApplicationDbContext db, INotificacionRepo notificacionRepo) : IEnviarRespuesta
    {
        private readonly ApplicationDbContext db = db;

        public async Task<bool> EnviarRespuesta(EnviarRespuestaDto data)
        {
            var emailConfig = await db.EmailConfigurations.FirstOrDefaultAsync();
            if (emailConfig == null)
                return false;

            emailConfig.SendEmail([data.Para], data.Cc, null, data.Asunto, $"{data.Mensaje}\n\n{data.Firma}", data.Archivo != null ? [data.Archivo] : null);

            var alerta = await (from als in db.AlertaSeguimientos
                                join s in db.Seguimientos on als.SeguimientoId equals s.Id
                                join n in db.NNAs on s.NNAId equals n.Id
                                join a in db.Alertas on als.AlertaId equals a.Id
                                join ea in db.TPEstadoAlerta on als.EstadoId equals ea.Id
                                join sca in db.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                where als.AlertaId == data.IdAlerta
                                select new
                                {
                                    Nombre = sca.CategoriaAlertaId + "." + sca.Indicador,
                                    ea.Id,
                                    s.NNAId,
                                    SeguimientoId = s.Id,
                                    NNANombre = $"{n.PrimerNombre ?? ""} {n.SegundoNombre ?? ""} {n.PrimerApellido ?? ""} {n.SegundoApellido ?? ""}",
                                    sca.Indicador
                                }).FirstOrDefaultAsync();

            var noti = await notificacionRepo.SetNotificacion(new()
            {
                TipoNotificacion = TipoNotificacion.RespuestasNotificacionesAlertas,
                // BUG-LZ-089: pasar IdSeguimiento para que SetNotificacion resuelva y notifique
                // al agente asignado del caso (ademas de los coordinadores).
                IdSeguimiento = alerta.SeguimientoId,
                TextoNotificacion = $"La alerta {alerta.Indicador} {alerta.Nombre} No. {alerta.Id:000000} del caso No. {alerta.NNAId:0000000} del NNA {alerta.NNANombre} ha recibido una respuesta."
            });

            return true;
        }
    }
}
