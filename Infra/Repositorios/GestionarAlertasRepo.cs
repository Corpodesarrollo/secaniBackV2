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
            var query = from s in db.Seguimientos
                        join n in db.NNAs on s.NNAId equals n.Id
                        group s by s.NNAId into g
                        select new { Id = g.Max(x => x.Id) };

            var alertasBase = (from q in query

                               join s in db.Seguimientos on q.Id equals s.Id
                               join n in db.NNAs on s.NNAId equals n.Id

                               join als in db.AlertaSeguimientos on s.Id equals als.SeguimientoId
                               join a in db.Alertas on als.AlertaId equals a.Id
                               join ea in db.TPEstadoAlerta on als.EstadoId equals ea.Id
                               join sca in db.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                               join ca in db.TPCategoriaAlerta on sca.CategoriaAlertaId equals ca.Id
                               join eapb in db.TPEAPB on n.EAPBId equals eapb.Id into eapbGroup
                               from eapb in eapbGroup.DefaultIfEmpty()
                               select new GestionarAlertasDto
                               {
                                   IdAlerta = als.Id,
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
                    Firma = dto.Firma
                };

                db.RespuestasAlerta.Add(respuestaAlerta);
                await db.SaveChangesAsync();

                if (dto.Archivo != null)
                {
                    var nombreArchivo = $"AdjuntoRespuesta-{Guid.NewGuid()}.{dto.Archivo.FileExtension}";
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
