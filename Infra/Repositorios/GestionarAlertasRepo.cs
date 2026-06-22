using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;
using Core.Services.StorageService;
using Microsoft.EntityFrameworkCore;
using static Core.Common.Estructuras;

namespace Infra.Repositorios
{
    public class GestionarAlertasRepo(ApplicationDbContext db, IStorageService _storageService, INotificacionRepo notificacionRepo) : IGestionarAlertas
    {
        public List<GestionarAlertasDto> ObtenerAlertas(string alias)
        {
            // HU SECANI-RQ07-HU03: la lista debe restringirse a las alertas de la entidad
            // del usuario. Convencion qa-login: alias = "NI" + NIT del TPEAPB. Si el alias
            // no tiene ese prefijo (o es "admin") no se filtra y se devuelven todas (rol
            // administrativo / global).
            int? eapbFiltro = null;
            if (!string.IsNullOrWhiteSpace(alias) && alias.StartsWith("NI", StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(alias.Substring(2), out var nit))
                {
                    eapbFiltro = db.TPEAPB.Where(e => e.NIT == nit).Select(e => (int?)e.Id).FirstOrDefault();
                }
            }

            // BUG-LZ 2026-06-20: antes se tomaba el ultimo seguimiento por NNA, lo que
            // ocultaba alertas de seguimientos anteriores aunque hubieran sido notificadas
            // a la EAPB y siguieran sin resolver. Repro: agente crea S1+alerta tipo 2
            // notificada -> EAPB ve la 2. Agente crea S2+alerta tipo 7 notificada -> EAPB
            // solo veia la 7. Ahora se toma el ultimo snapshot POR ALERTA (AlertaId-base)
            // para que cada alerta unica del NNA aparezca con su estado vigente.
            var ultimoSnapshotPorAlerta = from als in db.AlertaSeguimientos
                                          group als by als.AlertaId into g
                                          select g.Max(x => x.Id);

            // HU SECANI-RQ07-HU03 (extension): "mi entidad" incluye dos casos:
            //   (a) NNAs cuya EAPB del NNA es la del usuario logueado
            //   (b) NNAs con notificaciones (oficios) enviadas a la EAPB del usuario,
            //       aunque la EAPB del NNA sea otra. Caso real: LUCAS pertenece a UNIMEC
            //       pero recibio notificacion dirigida a Colsubsidio -> Colsubsidio debe
            //       poder gestionarla.
            // BUG-LZ 2026-06-20: las notis se persisten contra el AlertaSeguimientoId
            // (snapshot puntual). Como Design B re-snapshota por seguimiento, el snapshot
            // vigente puede ser otro distinto al que se notifico. Hay que traducir las
            // notis a AlertaId-base para que el filtro siga aplicando despues del re-snapshot.
            var alertasNotificadasAEntidad = eapbFiltro == null
                ? null
                : (from ne in db.NotificacionesEntidad
                   join asnap in db.AlertaSeguimientos on ne.AlertaSeguimientoId equals asnap.Id
                   where ne.EntidadId == eapbFiltro && !ne.IsDeleted
                   select asnap.AlertaId).Distinct();

            // BUG-LZ 2026-06-20: per HU SECANI-RQ07-HU04, una alerta solo recibe una
            // respuesta y eso la "cierra". Calculamos el set de AlertaId-base que ya
            // tienen al menos una respuesta para que el front oculte el boton "Enviar
            // respuesta". RespuestasAlerta vive por NotificacionEntidad; resolvemos
            // hacia el AlertaId-base via AlertaSeguimientos (snapshot puede cambiar
            // entre seguimientos, igual que el filtro de notis).
            // RespuestasAlerta.IdAlerta = AlertaSeguimiento.Id (snapshot puntual, no la
            // alerta-base). Resolvemos via AlertaSeguimientos para obtener el AlertaId-base.
            var alertasConRespuesta = (from r in db.RespuestasAlerta
                                       join asnap in db.AlertaSeguimientos on r.IdAlerta equals asnap.Id
                                       where !r.IsDeleted
                                       select asnap.AlertaId).Distinct().ToHashSet();

            var alertasBase = (from snapId in ultimoSnapshotPorAlerta
                               join als in db.AlertaSeguimientos on snapId equals als.Id
                               join s in db.Seguimientos on als.SeguimientoId equals s.Id
                               join n in db.NNAs on s.NNAId equals n.Id
                               join a in db.Alertas on als.AlertaId equals a.Id
                               join ea in db.TPEstadoAlerta on als.EstadoId equals ea.Id
                               join sca in db.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                               join ca in db.TPCategoriaAlerta on sca.CategoriaAlertaId equals ca.Id
                               join eapb in db.TPEAPB on n.EAPBId equals eapb.Id into eapbGroup
                               from eapb in eapbGroup.DefaultIfEmpty()
                               where eapbFiltro == null
                                     || n.EAPBId == eapbFiltro
                                     || alertasNotificadasAEntidad!.Contains(als.AlertaId)
                               select new GestionarAlertasDto
                               {
                                   IdAlerta = als.Id,
                                   AlertaIdBase = als.AlertaId,
                                   IdEstadoAlerta = ea.Id,
                                   IdAlertaSeguimiento = als.Id,
                                   IdSeguimiento = s.Id,
                                   Alerta = sca.CategoriaAlertaId + "." + sca.Indicador,
                                   NombreNNA = $"{n.PrimerNombre ?? ""} {n.SegundoNombre ?? ""} {n.PrimerApellido ?? ""} {n.SegundoApellido ?? ""}",
                                   NombreEAPB = eapb == null ? "" : eapb.Nombre,
                                   DocumentoNNA = $"{n.TipoIdentificacionId} {n.NumeroIdentificacion ?? ""}",
                                   Categoria = $"{ca.Id}. {ca.Nombre}",
                                   Subcategoria = sca.SubCategoriaAlerta,
                                   FechaNotificacion = s.FechaSeguimiento,
                                   Estado = ea.Nombre
                               }).ToList();

            alertasBase.ForEach(item =>
            {
                item.TextoEstado = item.IdEstadoAlerta switch
                {
                    4 => "RESUELTA",
                    1 or 2 or 3 or 5 => "SIN RESOLVER",
                    _ => "CERRADA"
                };

                item.ColorEstado = item.IdEstadoAlerta switch
                {
                    4 => "success",
                    1 or 2 => "warning",
                    3 or 5 => "danger",
                    _ => "secondary"
                };

                item.TieneRespuesta = alertasConRespuesta.Contains(item.AlertaIdBase);
            });

            return alertasBase;
        }

        public async Task<NotificacionEntidadDto?> GetNotificacionEntidad(int idAlerta)
        {
            // Bug 2026-06-17: throw "Notificacion no encontrada" generaba 500 que el front no
            // distinguia de error real -> modal caia a un template hardcoded con datos "Ejemplo".
            // Ahora retornar null para que el front muestre estado vacio explicito.
            var result = await db.NotificacionesEntidad.FirstOrDefaultAsync(x => x.AlertaSeguimientoId == idAlerta);
            if (result == null) return null;
            return GenericMapper.Map<NotificacionEntidad, NotificacionEntidadDto>(result);
        }

        public async Task<RespuestasAlertaDto> Alerta(int idAlerta)
        {
            try
            {
                var result = await db.RespuestasAlerta.FirstOrDefaultAsync(x => x.IdAlerta == idAlerta) ?? throw new Exception("Alerta no encontrada");
                var data = GenericMapper.Map<RespuestasAlerta, RespuestasAlertaDto>(result);
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public async Task<RespuestaResponse<bool>> EnviarRespuesta(EnviarRespuestaDto dto)
        {
            try
            {
                var respuestaAlerta = new RespuestasAlerta
                {
                    IdAlerta = dto.IdAlerta,
                    Para = dto.Para,
                    ConCopia = dto.Cc != null ? string.Join(",", dto.Cc) : null,
                    Asunto = dto.Asunto,
                    Mensaje = dto.Mensaje,
                    // BUG-LZ 2026-06-20: la columna Respuesta nunca se poblaba (DTO solo trae
                    // Mensaje) y GetNotificacionAlerta filtra por Respuesta -> modal vacio.
                    // Copiamos Mensaje en Respuesta para que el modal lo muestre.
                    Respuesta = dto.Mensaje,
                    Firma = dto.Firma
                };

                db.RespuestasAlerta.Add(respuestaAlerta);
                await db.SaveChangesAsync();

                if (dto.Archivo != null)
                {
                    // BUG-LZ 2026-06-20: antes se perdia el FileName original (solo guid + ext).
                    // El modal mostraba "pdf" como nombre. Preservar FileName para UI legible.
                    var safeName = !string.IsNullOrWhiteSpace(dto.Archivo.FileName)
                        ? dto.Archivo.FileName
                        : $"adjunto-{Guid.NewGuid()}.{dto.Archivo.FileExtension}";
                    var nombreArchivo = $"AdjuntoRespuesta-{respuestaAlerta.IdAlerta}-{safeName}";
                    var archivoAdjunto = new Adjuntos
                    {
                        NombreArchivo = nombreArchivo,
                        Referencia = respuestaAlerta.IdAlerta,
                        Tipo = TipoAdjunto.Respuesta,
                    };
                    db.Adjuntos.Add(archivoAdjunto);
                    await db.SaveChangesAsync();

                    await _storageService.UploadFileAsync(dto.Archivo.File, nombreArchivo);
                }

                var emailConfigurations = await db.EmailConfigurations.FirstOrDefaultAsync();
                if (emailConfigurations == null)
                    return new() { Estado = false, Descripcion = "No se ha configurado el envío de correos electrónicos" };

                var archivos = dto.Archivo != null ? new[] { dto.Archivo } : [];
                emailConfigurations.SendEmail([dto.Para], dto.Cc, null, dto.Asunto, $"{dto.Mensaje}</br>{dto.Firma}", archivos);

                // Bug 2026-06-17: el filtro era als.AlertaId == dto.IdAlerta. Tras el fix de
                // ID Alerta unico, dto.IdAlerta ahora es AlertaSeguimiento.Id (PK por fila), no
                // Alerta.Id (FK). El query daba null -> NullReferenceException en alerta.*.
                var alerta = await (from als in db.AlertaSeguimientos
                                    join s in db.Seguimientos on als.SeguimientoId equals s.Id
                                    join n in db.NNAs on s.NNAId equals n.Id
                                    join a in db.Alertas on als.AlertaId equals a.Id
                                    join ea in db.TPEstadoAlerta on als.EstadoId equals ea.Id
                                    join sca in db.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                    where als.Id == dto.IdAlerta
                                    select new
                                    {
                                        Nombre = sca.CategoriaAlertaId + "." + sca.Indicador,
                                        ea.Id,
                                        s.NNAId,
                                        SeguimientoId = s.Id,
                                        NNANombre = $"{n.PrimerNombre ?? ""} {n.SegundoNombre ?? ""} {n.PrimerApellido ?? ""} {n.SegundoApellido ?? ""}",
                                        sca.Indicador
                                    }).FirstOrDefaultAsync();

                if (alerta == null)
                    return new() { Estado = false, Descripcion = "No se encontro la alerta asociada al envio de respuesta." };

                var noti = await notificacionRepo.SetNotificacion(new()
                {
                    TipoNotificacion = TipoNotificacion.RespuestasNotificacionesAlertas,
                    // BUG-LZ-089: pasar IdSeguimiento para notificar tambien al agente asignado del caso.
                    IdSeguimiento = alerta.SeguimientoId,
                    TextoNotificacion = $"La alerta {alerta.Indicador} {alerta.Nombre} No. {alerta.Id:000000} del caso No. {alerta.NNAId:0000000} del NNA {alerta.NNANombre} ha recibido una respuesta."
                });

                return new() { Estado = true, Datos = true };
            }
            catch (Exception ex)
            {
                return new() { Estado = false, Descripcion = ex.Message };
            }
        }
    }
}
