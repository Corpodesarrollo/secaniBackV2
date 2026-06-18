using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Services.PDF;
using Core.Services.StorageService;
using Core.Utilities;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.Json;
using static Core.Common.Estructuras;
using Path = System.IO.Path;

namespace Infra.Repositories
{
    public class NotificacionRepo : INotificacionRepo
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly IAdjuntosRepo _adjuntosRepo;
        private readonly IStorageService _storageService;
        private readonly IReportesSIVIGILARepo _reportesSIVIGILARepo;
        private readonly SmtpClient clienteSmtp;
        private readonly string fromMail;
        private readonly ISeguimientoRepo _seguimientoRepo;

        public NotificacionRepo(ApplicationDbContext context, IAdjuntosRepo adjuntosRepo, IStorageService storageService, IReportesSIVIGILARepo reportesSIVIGILARepo, IWebHostEnvironment env, ISeguimientoRepo seguimientoRepo)
        {
            try
            {
                _context = context;
                _adjuntosRepo = adjuntosRepo;
                _storageService = storageService;
                _reportesSIVIGILARepo = reportesSIVIGILARepo;
                _seguimientoRepo = seguimientoRepo;
                _env = env;

                // Obtener configuraciones de correo
                var emailConfigurations = _context.EmailConfigurations.ToList();

                if (emailConfigurations.Count > 0)
                {
                    var emailConfiguration = emailConfigurations[0];
                    fromMail = emailConfiguration.UserName;
                    clienteSmtp = new SmtpClient(emailConfiguration.SmtpServer)
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(emailConfiguration.UserName, emailConfiguration.Password),
                        EnableSsl = emailConfiguration.EnableSsl,
                        Timeout = 15000
                    };
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<GetNotificacionResponse>> GetNotificacionUsuario(string AgenteDestinoId)
        {
            var umbralFecha = new DateTime(1900, 1, 1);
            var response = await (from un in _context.NotificacionesUsuarios
                                  join uDestino in _context.Users on un.AgenteDestinoId equals uDestino.Id
                                  join ruDestino in _context.UserRoles on uDestino.Id equals ruDestino.UserId
                                  join rDestino in _context.Roles on ruDestino.RoleId equals rDestino.Id
                                  // BUG-LZ-089: LEFT JOIN al origen. Las respuestas a alertas (tipo 4)
                                  // no tienen un usuario origen real; con INNER JOIN esas notis nunca
                                  // se mostraban. Con LEFT JOIN se muestran (origen queda en blanco).
                                  join uOrigenJ in _context.Users on un.AgenteOrigenId equals uOrigenJ.Id into uOrigenG
                                  from uOrigen in uOrigenG.DefaultIfEmpty()
                                  join ruOrigenJ in _context.UserRoles on uOrigen.Id equals ruOrigenJ.UserId into ruOrigenG
                                  from ruOrigen in ruOrigenG.DefaultIfEmpty()
                                  join rOrigenJ in _context.Roles on ruOrigen.RoleId equals rOrigenJ.Id into rOrigenG
                                  from rOrigen in rOrigenG.DefaultIfEmpty()
                                  where un.AgenteDestinoId == AgenteDestinoId && !un.IsDeleted
                                  // BUG-LZ-088: FechaNotificacion es DateTime no-nullable; las notis
                                  // viejas/otros tipos quedaban en 0001-01-01 (default) -> el modal
                                  // mostraba 01/01/0001. Usar DateCreated como respaldo cuando la
                                  // fecha esta sin setear (umbral 1900 cubre el default MinValue).
                                  orderby un.IsDeleted, (un.FechaNotificacion < umbralFecha ? un.DateCreated : un.FechaNotificacion) descending
                                  select new GetNotificacionResponse()
                                  {
                                      IdNotificacion = un.Id,
                                      IdSeguimiento = un.SeguimientoId,
                                      TipoNotificacion = (TipoNotificacion)un.TipoNotificacionId,
                                      AgenteDestino = uDestino.FullName,
                                      RolAgenteDestino = rDestino.Name,
                                      AgenteOrigen = uOrigen != null ? uOrigen.FullName : null,
                                      RolAgenteOrigen = rOrigen != null ? rOrigen.Name : null,
                                      FechaNotificacion = un.FechaNotificacion < umbralFecha ? (un.DateCreated ?? un.FechaNotificacion) : un.FechaNotificacion,
                                      TextoNotificacion = un.Asunto,
                                      Leida = un.IsDeleted,
                                      // BUG-LZ noti "Ver": la projection no devolvia Url -> el front
                                      // recibia url undefined y "Ver" no navegaba. Fallback a la ruta
                                      // del seguimiento cuando Url quedo sin setear pero hay SeguimientoId.
                                      Url = un.Url != null ? un.Url
                                            : (un.SeguimientoId > 0 ? "/gestion/detalle_seguimiento/" + un.SeguimientoId : null),
                                  }).Take(10).ToListAsync();

            return response;
        }

        public async Task<int> GetNumeroNotificacionUsuario(string AgenteDestinoId)
        {
            var count = await _context.NotificacionesUsuarios.Where(x => x.AgenteDestinoId == AgenteDestinoId && !x.IsDeleted).CountAsync();
            return count;
        }

        public async Task<bool> SetNotificacion(GetNotificacionResponse data)
        {
            try
            {
                var coordinadores = await (from u in _context.Users
                                           join ur in _context.UserRoles on u.Id equals ur.UserId
                                           join r in _context.Roles on ur.RoleId equals r.Id
                                           where r.Id == "311882D4-EAD0-4B0B-9C5D-4A434D49D16D" //rol coordinador
                                           select u).ToListAsync();

                if (data.TipoNotificacion == TipoNotificacion.AsignacionSolicitudesCuidadores)
                {
                    var userOrigen = await (from us in _context.Users
                                            join ur in _context.UserRoles on us.Id equals ur.UserId
                                            join r in _context.Roles on ur.RoleId equals r.Id
                                            where us.Id == data.AgenteOrigen
                                            select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    var userDestino = await (from us in _context.Users
                                             join ur in _context.UserRoles on us.Id equals ur.UserId
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             where us.Id == data.AgenteDestino
                                             select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    var asunto = data.TextoNotificacion
                            .Replace("-RolOrigen-", userOrigen.Name)
                            .Replace("-NombresOrigen-", userOrigen.FullName)
                            .Replace("-RolDestino-", userDestino.Name)
                            .Replace("-NombresDestino-", userDestino.FullName);

                    if (data.Administrador)
                        foreach (var coord in coordinadores)
                            _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                            {
                                TipoNotificacionId = (int)data.TipoNotificacion,
                                AgenteDestinoId = coord.Id,
                                AgenteOrigenId = data.AgenteOrigen,
                                Asunto = asunto,
                                Url = $"/gestion/detalle_seguimiento/{data.IdSeguimiento}"
                            });
                    else
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = data.AgenteDestino,
                            AgenteOrigenId = data.AgenteOrigen,
                            Asunto = asunto,
                            Url = $"/gestion/detalle_seguimiento/{data.IdSeguimiento}"
                        });

                }
                else if (data.TipoNotificacion == TipoNotificacion.Manual)
                {
                    var userOrigen = await (from us in _context.Users
                                            join ur in _context.UserRoles on us.Id equals ur.UserId
                                            join r in _context.Roles on ur.RoleId equals r.Id
                                            where us.Id == data.AgenteOrigen
                                            select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    var userDestino = await (from us in _context.Users
                                             join ur in _context.UserRoles on us.Id equals ur.UserId
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             where us.Id == data.AgenteDestino
                                             select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    var nna = await (from s in _context.Seguimientos
                                     join n in _context.NNAs on s.NNAId equals n.Id
                                     where s.Id == data.IdSeguimiento
                                     select new { n.Id }).FirstOrDefaultAsync();


                    _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                    {
                        TipoNotificacionId = (int)data.TipoNotificacion,
                        AgenteDestinoId = data.AgenteDestino,
                        AgenteOrigenId = data.AgenteOrigen,
                        Asunto = $"El {userOrigen.Name} {userOrigen.FullName} le ha asignado el caso No. {nna.Id:000000} al {userDestino.Name} {userDestino.FullName}",
                        Url = $"/gestion/detalle_seguimiento/{data.IdSeguimiento}",
                    });

                }
                else if (data.TipoNotificacion == TipoNotificacion.ActivacionInactivacionPerfil)
                {
                    var user = await (from us in _context.Users
                                      join ur in _context.UserRoles on us.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where us.Id == data.IdAgenteOrigen
                                      select new { us.FullName, us.Activo, r.Name }).FirstOrDefaultAsync();

                    foreach (var coord in coordinadores)
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = coord.Id,
                            AgenteOrigenId = data.IdAgenteOrigen,
                            Asunto = $"El {user.Name} {user.FullName} se ha {(user.Activo == true ? "activado" : "inactivado")} en el sistema ",
                            Url = $"/administracion/permisos"
                        });
                }
                else if (data.TipoNotificacion == TipoNotificacion.DiasAusencia)
                {
                    var user = await (from us in _context.Users
                                      join ur in _context.UserRoles on us.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where us.Id == data.IdAgenteOrigen
                                      select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    foreach (var coord in coordinadores)
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = coord.Id,
                            AgenteOrigenId = data.IdAgenteOrigen,
                            // BUG-LZ-079: la lista del campanita ordena por FechaNotificacion DESC y
                            // toma top-10; si queda NULL la notif cae al fondo y puede no mostrarse.
                            FechaNotificacion = data.FechaNotificacion == default ? DateTime.UtcNow : data.FechaNotificacion,
                            Asunto = $"El {user.Name} {user.FullName} {data.TextoNotificacion}",
                            Url = $"/administracion/permisos"
                        });
                }
                else if (data.TipoNotificacion == TipoNotificacion.ProgramacionHorario)
                {
                    // BUG-LZ-079: al programar/actualizar el horario laboral ("Mi semana") el
                    // agente debe notificar a los coordinadores. Antes no existia esta rama.
                    var user = await (from us in _context.Users
                                      join ur in _context.UserRoles on us.Id equals ur.UserId
                                      join r in _context.Roles on ur.RoleId equals r.Id
                                      where us.Id == data.IdAgenteOrigen
                                      select new { us.FullName, r.Name }).FirstOrDefaultAsync();

                    foreach (var coord in coordinadores)
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = coord.Id,
                            AgenteOrigenId = data.IdAgenteOrigen,
                            FechaNotificacion = data.FechaNotificacion == default ? DateTime.UtcNow : data.FechaNotificacion,
                            Asunto = $"El {user?.Name} {user?.FullName} {data.TextoNotificacion}",
                            Url = $"/administracion/permisos"
                        });
                }
                else if (data.TipoNotificacion == TipoNotificacion.RespuestasNotificacionesAlertas)
                {
                    // BUG-LZ-089: antes no existia rama -> responder una alerta no generaba
                    // notificacion alguna. Decision negocio: notificar a Coordinadores + agente(s)
                    // asignado(s) del caso (y al IdAgenteDestino explicito si el caller lo manda).
                    var asunto = !string.IsNullOrWhiteSpace(data.TextoNotificacion)
                        ? data.TextoNotificacion
                        : "Una alerta del caso ha recibido una respuesta.";
                    var url = data.IdSeguimiento > 0
                        ? $"/gestion/detalle_seguimiento/{data.IdSeguimiento}"
                        : "/gestion/seguimientos";

                    var destinatarios = coordinadores.Select(c => c.Id).ToList();

                    if (!string.IsNullOrWhiteSpace(data.IdAgenteDestino))
                        destinatarios.Add(data.IdAgenteDestino);

                    if (data.IdSeguimiento > 0)
                    {
                        var agentesCaso = await (from ua in _context.UsuarioAsignados
                                                 where ua.SeguimientoId == data.IdSeguimiento && ua.Activo
                                                 select ua.UsuarioId).ToListAsync();
                        destinatarios.AddRange(agentesCaso);
                    }

                    foreach (var destinoId in destinatarios.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct())
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = destinoId,
                            AgenteOrigenId = data.IdAgenteOrigen ?? "",
                            FechaNotificacion = DateTime.UtcNow,
                            Asunto = asunto,
                            Url = url
                        });
                }
                else if (data.TipoNotificacion == TipoNotificacion.AsignacionReasignacion)
                {
                    // BUG-LZ-090: notificar al agente al que se le reasigna un caso (antes la
                    // reasignacion no generaba notificacion, por eso un agente podia tener casos
                    // reasignados pero 0 notificaciones).
                    var caso = await (from s in _context.Seguimientos
                                      where s.Id == data.IdSeguimiento
                                      select new { s.NNAId }).FirstOrDefaultAsync();
                    var asunto = !string.IsNullOrWhiteSpace(data.TextoNotificacion)
                        ? data.TextoNotificacion
                        : $"Le han reasignado el caso No. {(caso != null ? caso.NNAId : 0):000000}";

                    if (!string.IsNullOrWhiteSpace(data.IdAgenteDestino))
                        _context.NotificacionesUsuarios.Add(new NotificacionesUsuario
                        {
                            TipoNotificacionId = (int)data.TipoNotificacion,
                            AgenteDestinoId = data.IdAgenteDestino,
                            AgenteOrigenId = data.IdAgenteOrigen ?? "",
                            FechaNotificacion = DateTime.UtcNow,
                            Asunto = asunto,
                            Url = data.IdSeguimiento > 0 ? $"/gestion/detalle_seguimiento/{data.IdSeguimiento}" : "/gestion/seguimientos"
                        });
                }

                // BUG-LZ-088: garantizar FechaNotificacion en todas las ramas (varias no la seteaban
                // -> el modal mostraba 01/01/0001). Asignar a las notis nuevas que aun esten en default.
                foreach (var entry in _context.ChangeTracker.Entries<NotificacionesUsuario>())
                {
                    if (entry.State == EntityState.Added && entry.Entity.FechaNotificacion == default(DateTime))
                    {
                        entry.Entity.FechaNotificacion = data.FechaNotificacion == default ? DateTime.UtcNow : data.FechaNotificacion;
                    }
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // BUG-LZ-079: antes el catch retornaba false en silencio -> 0 notis tipo 7 sin
                // pista. Loggear stacktrace al stderr para que aparezca en `docker logs`.
                Console.Error.WriteLine($"[SetNotificacion] Tipo={data?.TipoNotificacion} IdAgenteOrigen={data?.IdAgenteOrigen} ex={ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public async Task<RespuestaResponse<long>> GenerarOficioNotificacion(OficioNotificacionRequest request)
        {
            try
            {
                var notificacionEntidad = await (from ne in _context.NotificacionesEntidad
                                                 where ne.Id == request.Id
                                                 select ne).FirstOrDefaultAsync();

                var entidad = await (from ent in _context.TPEAPB
                                     where ent.Id == request.IdEntidad
                                     select ent).FirstOrDefaultAsync();

                var alerta = await (from als in _context.AlertaSeguimientos
                                    where als.Id == request.IdAlertaSeguimiento
                                    select als).FirstOrDefaultAsync();

                var nna = await (from Tnna in _context.NNAs
                                 where Tnna.Id == request.IdNNA
                                 select Tnna).FirstOrDefaultAsync();

                var user = await (from us in _context.Users
                                  where us.Email == request.UserName
                                  select us).FirstOrDefaultAsync();

                if (user == null)
                    return new() { Estado = false, Descripcion = "El usuario no existe" };

                if (entidad == null)
                    return new() { Estado = false, Descripcion = "La entidad no existe" };

                if (alerta == null)
                    return new() { Estado = false, Descripcion = "La alerta no existe" };

                if (nna == null)
                    return new() { Estado = false, Descripcion = "El NNA no existe" };

                if (notificacionEntidad == null)
                {
                    notificacionEntidad = new NotificacionEntidad();
                }

                notificacionEntidad.EntidadId = request.IdEntidad;
                notificacionEntidad.AlertaSeguimientoId = request.IdAlertaSeguimiento;
                notificacionEntidad.Asunto = request.Asunto;
                notificacionEntidad.Cierre = request.Cierre;
                notificacionEntidad.CiudadEnvio = request.CiudadEnvio;
                notificacionEntidad.FechaEnvio = request.FechaEnvio;
                notificacionEntidad.Membrete = request.Membrete;
                notificacionEntidad.Ciudad = request.Ciudad;
                notificacionEntidad.Mensaje = request.Mensaje;
                notificacionEntidad.Comentario = request.Comentario;
                //notificacionEntidad.NNAs = nna;
                notificacionEntidad.NNAId = nna.Id;
                notificacionEntidad.Firmajpg = request.FirmaJpg;

                if (notificacionEntidad.Id == 0)
                {
                    notificacionEntidad.CreatedByUserId = user.Id;
                    notificacionEntidad.DateCreated = DateTime.Now;

                    _context.NotificacionesEntidad.Add(notificacionEntidad);
                }
                else
                {
                    notificacionEntidad.UpdatedByUserId = user.Id;
                    notificacionEntidad.DateUpdated = DateTime.Now;

                    _context.NotificacionesEntidad.Update(notificacionEntidad);
                }

                _context.SaveChanges();

                return new()
                {
                    Estado = true,
                    Descripcion = "Oficio creado correctamente",
                    Datos = notificacionEntidad.Id
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Estado = false,
                    Descripcion = "Error al crear el oficio",
                };
            }

        }

        public void EliminarNotificacion(EliminarNotificacionRequest request)
        {
            NotificacionesUsuario? notificacion = (from ne in _context.NotificacionesUsuarios
                                                   where ne.Id == request.IdNotificacionUsuario
                                                   select ne).FirstOrDefault();

            if (notificacion != null)
            {
                notificacion.IsDeleted = true;
                notificacion.DeletedByUserId = request.IdUsuario;
                notificacion.DateDeleted = DateTime.Now;

                _context.Update(notificacion);
                _context.SaveChanges();
            }
        }

        public async Task<RespuestaResponse<string>> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request)
        {
            try
            {
                var bodyHtml = @"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <style>
                                @page {
                                  margin-top: 5cm;
                                  margin-right: 2cm;
                                  margin-bottom: 3cm;
                                  margin-left: 2cm;
                                }
                                body { 
                                    font-family: Arial; 
                                    line-height: 1.5;
                                    width: 100%;
                                    box-sizing: border-box;
                                    padding: 0;
                                    margin: 0;
                                }
                                .barrera {
                                    border-left: 3px solid #ccc;
                                    padding-left: 10px;
                                    margin: 15px 0;
                                }
                            </style>
                        </head>
                        <body>
                            <div style='margin-bottom: 20px;'>
                                <p>{Ciudad}, {FechaEnvio}</p>
                            </div>

                            <div style='margin-bottom: 20px;'>
                                {Membrete}<br>
                                {NombreEntidad}<br>
                                {CiudadEnvio}
                            </div>

                            <div style='margin: 20px 0;'>
                                <strong>Asunto:</strong> {Asunto}
                            </div>

                            <div style='margin-bottom: 15px; text-align: justify;'>
                                {Mensaje}
                            </div>

                            <div style='margin-bottom: 15px; text-align: justify;'>
                                En este sentido el Ministerio de Salud y Protección Social en el marco de la <strong>Estrategia de Seguimiento Nacional de Cáncer Infantil</strong> ha identificado a través del padre, acudiante o representante legal {NombrePariente} la siguiente barrera:
                            </div>

                            <div class='barrera' style='margin-bottom: 15px; text-align: justify;'>
                                <strong>{Alerta}:</strong> {DescripcionAlerta}
                            </div>

                            <div style='margin-bottom: 15px; text-align: justify;'>
                                {Comentario}
                            </div>

                            <div style='margin: 20px 0;'>
                                <div><strong>Nombre:</strong> {NombreNNA}</div>
                                <div><strong>Identificación:</strong> {IdentificaciónNNA}</div>
                                <div><strong>Edad:</strong> {EdadNNA}</div>
                                <div><strong>Diagnóstico:</strong> {DiagnosticoNNA}</div>
                                <div><strong>Teléfono del acudiente:</strong> {TelefonoNNA}</div>
                            </div>

                            <div style='margin-bottom: 15px; text-align: justify;'>
                                {Cierre}
                            </div>

                            <div style='margin-bottom: 15px; text-align: justify;'>
                                {Firma}
                            </div>

                            <div style='margin-bottom: 15px;'></div>
                        </body>
                        </html>
                    ";

                var notificacionEntidadPlantilla = (from ne in _context.NotificacionesEntidad
                                                    join ent in _context.TPEAPB on ne.EntidadId equals ent.Id
                                                    join alseg in _context.AlertaSeguimientos on ne.AlertaSeguimientoId equals alseg.Id
                                                    join s in _context.Seguimientos on alseg.SeguimientoId equals s.Id
                                                    join c in _context.ContactoNNAs on s.ContactoNNAId equals c.Id
                                                    join al in _context.Alertas on alseg.AlertaId equals al.Id
                                                    join sca in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals sca.Id
                                                    join ca in _context.TPCategoriaAlerta on sca.CategoriaAlertaId equals ca.Id
                                                    join nna in _context.NNAs on ne.NNAId equals nna.Id
                                                    join d in _context.CIE10s on nna.DiagnosticoId equals d.Id
                                                    where ne.Id == request.IdNotificacion
                                                    select new NotificacionEntidadPlantilla()
                                                    {
                                                        IdSeguimiento = s.Id,
                                                        Asunto = ne.Asunto,
                                                        Cierre = ne.Cierre,
                                                        Ciudad = ne.Ciudad,
                                                        CiudadEnvio = ne.CiudadEnvio,
                                                        ComentarioNotificacion = ne.Comentario,
                                                        Alerta = ca.Id + ". " + ca.Nombre,
                                                        DescripcionAlerta = sca.Indicador + ". " + sca.SubCategoriaAlerta,
                                                        DiagnosticoNNA = d.Nombre,
                                                        DocumentoNNA = nna.TipoIdentificacionId + " " + nna.NumeroIdentificacion,
                                                        FechaNacimientoNNA = nna.FechaNacimiento,
                                                        FechaEnvio = ne.FechaEnvio ?? DateTime.Now,
                                                        Firma = ne.Firmajpg,
                                                        Membrete = ne.Membrete,
                                                        Mensaje = ne.Mensaje,
                                                        NombrePariente = c.Nombres,
                                                        NombreEntidad = ent.Nombre,
                                                        PrimerApellidoNNA = nna.PrimerApellido,
                                                        PrimerNombreNNA = nna.PrimerNombre,
                                                        SegundoApellidoNNA = nna.SegundoApellido,
                                                        SegundoNombreNNA = nna.SegundoNombre,
                                                        TelefonoAcudienteNNA = c.Telefonos
                                                    }).FirstOrDefault();

                if (notificacionEntidadPlantilla != null)
                {

                    bodyHtml = bodyHtml.Replace("{Ciudad}", notificacionEntidadPlantilla.Ciudad)
                                        .Replace("{FechaEnvio}", notificacionEntidadPlantilla.FechaEnvio.ToString("MMMM dd 'de' yyyy", new CultureInfo("es-ES")))
                                        .Replace("{Membrete}", notificacionEntidadPlantilla.Membrete ?? "")
                                        .Replace("{NombreEntidad}", notificacionEntidadPlantilla.NombreEntidad ?? "")
                                        .Replace("{CiudadEnvio}", notificacionEntidadPlantilla.CiudadEnvio ?? "")
                                        .Replace("{Asunto}", notificacionEntidadPlantilla.Asunto ?? "")
                                        .Replace("{Mensaje}", notificacionEntidadPlantilla.Mensaje ?? "")
                                        .Replace("{NombrePariente}", notificacionEntidadPlantilla.NombrePariente ?? "")
                                        .Replace("{Alerta}", notificacionEntidadPlantilla.Alerta ?? "")
                                        .Replace("{DescripcionAlerta}", notificacionEntidadPlantilla.DescripcionAlerta ?? "")
                                        .Replace("{Comentario}", notificacionEntidadPlantilla.ComentarioNotificacion ?? "")
                                        .Replace("{NombreNNA}", string.Concat(notificacionEntidadPlantilla.PrimerNombreNNA, " ", notificacionEntidadPlantilla.SegundoNombreNNA, " ",
                                        notificacionEntidadPlantilla.PrimerApellidoNNA, " ", notificacionEntidadPlantilla.SegundoApellidoNNA))
                                        .Replace("{IdentificaciónNNA}", notificacionEntidadPlantilla.DocumentoNNA ?? "")
                                        .Replace("{EdadNNA}", Funciones.CalcularEdad(notificacionEntidadPlantilla.FechaNacimientoNNA))
                                        .Replace("{DiagnosticoNNA}", notificacionEntidadPlantilla.DiagnosticoNNA.ToString())
                                        .Replace("{TelefonoNNA}", notificacionEntidadPlantilla.TelefonoAcudienteNNA ?? "")
                                        .Replace("{Cierre}", notificacionEntidadPlantilla.Cierre ?? "")
                                        .Replace("{Firma}", notificacionEntidadPlantilla.Firma ?? "");

                    bodyHtml = bodyHtml.Replace("<p", "<div style='margin:0;padding:0'").Replace("</p>", "</div>").Replace("\u00A0", " ").Replace("&nbsp;", " ");

                    // Cargar las imágenes del encabezado y pie de página
                    var imagePathHeader = Path.Combine(_env.WebRootPath, "assets", "header.png");
                    var imageBytesHeader = File.ReadAllBytes(imagePathHeader);
                    var base64ImageHeader = Convert.ToBase64String(imageBytesHeader);

                    var imagePathFooter = Path.Combine(_env.WebRootPath, "assets", "footer.png");
                    var imageBytesFooter = File.ReadAllBytes(imagePathFooter);
                    var base64ImageFooter = Convert.ToBase64String(imageBytesFooter);

                    var headerHtml = $@"
                        <div style='width: 100%; text-align:right; margin-bottom: 20px;'>
                            <img src='data:image/png;base64,{base64ImageHeader}' style='height: 80px;'/>
                        </div>
                    ";

                    var footerHtml = $@"
                        <div style='width: 100%;'>
                            <img src='data:image/png;base64,{base64ImageFooter}' style='height: 120px;'/>
                        </div>
                    ";

                    var config = new PdfConfig(56, 56, 56, 56) // margen inferior 100px o más
                    {
                        PageSize = PageSize.LETTER,
                        HeaderHtmlContent = headerHtml,
                        FooterHtmlContent = footerHtml,
                        MostrarNumeracion = true,
                    };

                    byte[] pdfBytes = PDFService.PdfToHtml(bodyHtml, config);

                    // BUG-LZ-086: si Para esta vacio, SendMailAsync truena con "Cannot send a message
                    // with no recipients" -> el cuerpo del error se perdia en catch ambiguo. Validar antes.
                    if (request.Para == null || request.Para.Length == 0)
                    {
                        return new RespuestaResponse<string>()
                        {
                            Estado = false,
                            Descripcion = "No se seleccionaron destinatarios para el correo (campo Para vacio).",
                            Datos = null
                        };
                    }

                    List<EmailConfiguration> emailConfigurations = _context.EmailConfigurations.ToList();

                    if (emailConfigurations.Count == 0)
                    {
                        // BUG-LZ-086: antes el codigo entraba al if Count>0 y, si la tabla estaba vacia,
                        // saltaba el bloque silenciosamente y devolvia "enviado correctamente". Ahora
                        // se reporta el motivo real para que el front muestre el error.
                        return new RespuestaResponse<string>()
                        {
                            Estado = false,
                            Descripcion = "No hay configuracion SMTP cargada (tabla EmailConfigurations vacia).",
                            Datos = null
                        };
                    }

                    {
                        EmailConfiguration emailConfiguration = emailConfigurations[0];
                        SmtpClient clienteSmtp = new(emailConfiguration.SmtpServer)
                        {
                            Port = 587, // Puerto SMTP
                            Credentials = new NetworkCredential(emailConfiguration.UserName, emailConfiguration.Password),
                            EnableSsl = emailConfiguration.EnableSsl, // Habilitar SSL
                            Timeout = 15000 // BUG-LZ-056: default 100s cuelga UI; falla rápida con try/catch wrapper
                        };

                        // Creación del mensaje de correo
                        MailMessage mensaje = new()
                        {
                            From = new MailAddress(emailConfiguration.UserName),
                            Subject = request.Asunto,
                            IsBodyHtml = true, // Cambiar a true si el cuerpo del correo es HTML
                        };

                        var body = request.Mensaje;

                        if (request.Enlace != null)
                            body += $@"
                                <br><br>
                                Para dar respuesta a la notificación, haga clic en el siguiente enlace:<br>
                                <a href='{request.Enlace}' style='color: #007bff; text-decoration: none; font-weight: bold;'>
                                    Responder a la notificación
                                </a>";

                        if (!string.IsNullOrEmpty(request.Comentario))
                            body += "<br><br>Comentario: " + request.Comentario;

                        if (!string.IsNullOrEmpty(request.Firma))
                            body += "<br><br>" + request.Firma;

                        mensaje.Body = body;

                        if (request.Para.Length > 0)
                            foreach (var item in request.Para)
                                mensaje.To.Add(item);

                        if (request.ConCopia.Length > 0)
                            foreach (var item in request.ConCopia)
                                mensaje.CC.Add(item);

                        // Agregar el archivo adjunto del oficio de notificación
                        var nombreOficio = $"OficioNotificacion-{Guid.NewGuid()}.pdf";
                        var adjuntoOficio = new Adjuntos
                        {
                            NombreArchivo = nombreOficio,
                            Tipo = TipoAdjunto.Notificacion,
                            Referencia = request.IdNotificacion
                        };
                        _context.Adjuntos.Add(adjuntoOficio);
                        await _context.SaveChangesAsync();

                        // Guardar el archivo adjunto en el almacenamiento
                        await _storageService.UploadFileAsync(pdfBytes, nombreOficio);

                        // Crear y agregar el archivo adjunto desde byte[]
                        using var ms = new MemoryStream(pdfBytes.ToArray());
                        Attachment adjunto = new(ms, "OficioNotificacion.pdf", MediaTypeNames.Application.Octet);
                        mensaje.Attachments.Add(adjunto);

                        var fileByte = pdfBytes.ToArray();
                        //await _storageService.UploadFileAsync(fileByte, $"OficioNotificacion-{request.IdNotificacion}.pdf");

                        if (request.Adjunto != null)
                        {
                            // Agregar el archivo adjunto adicional
                            var nombreAdjunto = $"AdjuntoEmail-{Guid.NewGuid()}.pdf";
                            var adjuntoEmail = new Adjuntos
                            {
                                NombreArchivo = nombreAdjunto,
                                Tipo = TipoAdjunto.Notificacion,
                                Referencia = request.IdNotificacion
                            };
                            _context.Adjuntos.Add(adjuntoEmail);
                            await _context.SaveChangesAsync();

                            // Guardar el archivo adjunto adicional en el almacenamiento
                            await _storageService.UploadFileAsync(request.Adjunto.File, nombreAdjunto);

                            using var ms2 = new MemoryStream(request.Adjunto.File);
                            Attachment adjunto2 = new(ms2, $"{request.Adjunto.FileName}", MediaTypeNames.Application.Octet);
                            mensaje.Attachments.Add(adjunto2);

                            // Guardar el archivo en el almacenamiento
                            var fileByte2 = request.Adjunto.File;
                            //await _storageService.UploadFileAsync(fileByte2, $"Adjunto-{request.IdNotificacion}.pdf");

                            // BUG-LZ-056: SendMailAsync no bloquea hilo; Timeout=15s en SmtpClient evita cuelgue
                            await clienteSmtp.SendMailAsync(mensaje);
                        }
                        else
                        {
                            await clienteSmtp.SendMailAsync(mensaje);
                        }
                    }

                    return new()
                    {
                        Estado = true,
                        Descripcion = "Oficio de notificacion enviado correctamente",
                        Datos = "Oficio de notificacion enviado correctamente",
                    };
                }
                else
                {
                    return new RespuestaResponse<string>()
                    {
                        Estado = false,
                        Descripcion = "oficio de notificacion no encontrado",
                        Datos = "oficio de notificacion no encontrado",
                    };
                }
            }
            catch (Exception ex)
            {
                return new RespuestaResponse<string>()
                {
                    Estado = false,
                    Descripcion = ex.Message,
                    Datos = ex.Message,
                };
            }
        }

        public async Task<RespuestaResponse<OficioNotificacionRequest>> VerOficioNotificacion(long id)
        {
            var notificacion = await (from ne in _context.NotificacionesEntidad
                                      where ne.AlertaSeguimientoId == id
                                      select ne).FirstOrDefaultAsync();

            if (notificacion == null)
                return new()
                {
                    Estado = false,
                    Descripcion = "No se encontró la notificación",
                    Datos = null
                };

            return new()
            {
                Estado = true,
                Descripcion = "Notificación encontrada",
                Datos = new()
                {
                    Id = notificacion.Id,
                    Asunto = notificacion.Asunto,
                    CiudadEnvio = notificacion.CiudadEnvio,
                    Comentario = notificacion.Comentario,
                    FechaEnvio = notificacion.FechaEnvio ?? DateTime.Now,
                    Mensaje = notificacion.Mensaje,
                    IdAlertaSeguimiento = notificacion.AlertaSeguimientoId ?? 0,
                    IdNNA = notificacion.NNAId ?? 0,
                    Cierre = notificacion.Cierre,
                    Ciudad = notificacion.Ciudad,
                    Firma = notificacion.Firmajpg,
                    FirmaJpg = notificacion.Firmajpg,
                    IdEntidad = notificacion.EntidadId ?? 0,
                    Membrete = notificacion.Membrete,
                    UserName = notificacion.CreatedByUserId,
                }
            };
        }

        public async Task<RespuestaResponse<bool>> NotificacionRespuesta(NotificacionRespuestaDto data)
        {
            try
            {
                // crear la notificacion de respuesta
                NotificacionRespuesta entityNotificacion = new()
                {
                    Cargo = data.Cargo,
                    Correo = data.Correo,
                    Entidad = data.Entidad,
                    NombreFuncionario = data.NombreFuncionario,
                    Respuesta = data.Respuesta,
                    Telefono = data.Telefono
                };

                _context.NotificacionRespuesta.Add(entityNotificacion);
                await _context.SaveChangesAsync();

                // cargar el adjunto con adjuntorepo
                if (data.Archivo != null)
                {
                    var ext = Path.GetExtension(data.Archivo.FileName);
                    var nombreAdjunto = $"AdjuntoRespuestaNotificacion-{Guid.NewGuid()}{ext}";
                    AdjuntosDto adjunto = new()
                    {
                        NombreArchivo = nombreAdjunto,
                        Tipo = TipoAdjunto.RespuestaNotificacion,
                        Referencia = data.IdNotificacion
                    };

                    var idAdjunto = await _adjuntosRepo.AddAdjunto(adjunto);

                    entityNotificacion.IdAdjunto = idAdjunto;
                    await _context.SaveChangesAsync();

                    // guardar adjunto en storage account
                    using var stream = new MemoryStream();
                    await data.Archivo.CopyToAsync(stream);
                    var fileByte = stream.ToArray();

                    await _storageService.UploadFileAsync(fileByte, nombreAdjunto);
                }

                return new()
                {
                    Estado = true,
                    Descripcion = "Notificación de respuesta guardada correctamente",
                    Datos = true
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Estado = false,
                    Descripcion = ex.Message,
                    Datos = false
                };
            }
        }


        public List<GetNotificacionesEntidadResponse> RepoNotificacionEntidadCasos(long entidadId, int alertaSeguimientoId, int nnaId)
        {
            List<GetNotificacionesEntidadResponse> notificacionEntidad = (from ne in _context.NotificacionesEntidad
                                                                          where ne.EntidadId == entidadId
                                                                          && ne.AlertaSeguimientoId == alertaSeguimientoId
                                                                          && ne.NNAId == nnaId

                                                                          select new GetNotificacionesEntidadResponse()
                                                                          {
                                                                              EntidadId = ne.EntidadId ?? 0,
                                                                              CiudadEnvio = ne.CiudadEnvio,
                                                                              FechaEnvio = ne.FechaEnvio ?? DateTime.Now,
                                                                              AlertaSeguimientoId = ne.AlertaSeguimientoId ?? 0,
                                                                              NNAId = ne.NNAId ?? 0,
                                                                              Ciudad = ne.Ciudad,
                                                                              EmailConfigurationId = ne.EmailConfigurationId ?? 0,
                                                                              EmailPara = ne.EmailPara,
                                                                              EmailCC = ne.EmailCC,
                                                                              PlantillaId = ne.PlantillaId,
                                                                              Asunto = ne.Asunto,
                                                                              Mensaje = ne.Mensaje,
                                                                              EnlaceParaRecibirRespuestas = ne.EnlaceParaRecibirRespuestas,
                                                                              Comentario = ne.Comentario,
                                                                              Firmajpg = ne.Firmajpg,
                                                                              ArchivoAdjunto = ne.ArchivoAdjunto
                                                                          }
                                                                          ).ToList();

            return notificacionEntidad;
        }


        public List<GetListaCasosResponse> RepoListaCasosNotificacion(string eapbId, int epsId)
        {
            // Bug 2026-06-17: el filtro de EAPBId estaba comentado y solo filtraba por EPSId.
            // El frontend (casos-entidad) pasaba eapbId="1" + epsId=1 hardcoded -> usuarios
            // ET/EAPB veian la tabla vacia. Ahora se aplica OR real entre los dos filtros y
            // se parsea eapbId como int (NNA.EAPBId es int?).
            int? eapbIdNum = int.TryParse(eapbId, out var parsed) && parsed > 0 ? parsed : null;
            int? epsIdNum = epsId > 0 ? epsId : (int?)null;

            List<GetListaCasosResponse> listaCasos = (from n in _context.NNAs
                                                      join s in _context.Seguimientos on n.Id equals s.NNAId
                                                      join a in _context.AlertaSeguimientos on s.Id equals a.SeguimientoId

                                                      where (eapbIdNum != null && n.EAPBId == eapbIdNum)
                                                         || (epsIdNum != null && n.EPSId == epsIdNum)
                                                      group new { n, s, a } by new
                                                      {
                                                          NNAId = n.Id,
                                                          n.FechaNotificacionSIVIGILA,
                                                          n.EAPBId,
                                                          Nombre = n.PrimerNombre + " " + n.SegundoNombre + " " + n.PrimerApellido + " " + n.SegundoApellido
                                                      } into g
                                                      select new
                                                      {
                                                          g.Key.NNAId,
                                                          g.Key.FechaNotificacionSIVIGILA,
                                                          g.Key.Nombre,
                                                          g.Key.EAPBId,
                                                          Seguimientos = g.Select(x => x.s).OrderByDescending(sg => sg.FechaSeguimiento).ToList(),  // Convertir a lista
                                                          Estados = g.Select(x => x.a.EstadoId).Distinct()
                                                      }).AsEnumerable() // Cambiar a evaluación en el cliente
              .Select(g => new GetListaCasosResponse
              {
                  NNAId = g.NNAId,
                  SeguimientoId = g.Seguimientos.FirstOrDefault().Id,
                  FechaNotificacionSIVIGILA = g.FechaNotificacionSIVIGILA,
                  Nombre = g.Nombre,
                  EAPBId = g.EAPBId,
                  ObservacionesSolicitante = g.Seguimientos.FirstOrDefault()?.ObservacionesSolicitante,
                  EstadoAlertasIds = string.Join(",", g.Estados),
                  EstadoSeguimientoId = g.Seguimientos.FirstOrDefault().EstadoId

              }).ToList();
            return listaCasos;
        }

        public List<NotificacionResponse> GetNotificacionAlerta(long AlertaId)
        {
            List<NotificacionResponse> response = (from un in _context.Notificacions
                                                   join ent in _context.Entidades on un.EntidadId equals ent.Id
                                                   where un.AlertaSeguimientoId == AlertaId && !un.IsDeleted
                                                   select new NotificacionResponse()
                                                   {
                                                       EntidadNotificada = ent.Nombre,
                                                       FechaNotificacion = un.FechaNotificacion,
                                                       FechaRespuesta = un.FechaRespuesta,
                                                       Respuesta = un.RespuestaEntidad,
                                                       AsuntoNotificacion = un.Asunto
                                                   }).ToList();

            return response;
        }



        /// <summary>
        /// Metodo principal para el envio del correo
        /// </summary>
        /// <param name="Para"></param>
        /// <param name="ConCopia"></param>
        /// <param name="Asunto"></param>
        /// <param name="Body"></param>
        /// <param name="Adjuntos"></param>
        /// <returns></returns>

        public async Task<string> PlantillaCorreo(string[] Para, string[] ConCopia, string Asunto, string Body, string[] Adjuntos, Attachment adjuntoPDF = null)
        {
            try
            {
                if (Body != null)
                {
                    // Configuración de Puppeteer (si es necesario)
                    await new BrowserFetcher().DownloadAsync();

                    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true
                    });

                    await using var page = await browser.NewPageAsync();
                    await page.SetContentAsync(Body);
                    await browser.CloseAsync();

                    Console.WriteLine("INICIO DEL ENVIO");



                    // Creación del mensaje de correo
                    MailMessage mensaje = new()
                    {
                        From = new MailAddress(fromMail),
                        Subject = Asunto,
                        Body = Body,
                        IsBodyHtml = true
                    };

                    // Agregar destinatarios
                    if (Para.Length > 0)
                    {
                        foreach (var item in Para)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                Console.WriteLine($"El correo para => {item}");
                                mensaje.To.Add(item);
                            }
                            else
                            {
                                Console.WriteLine("Elemento vacío o nulo en el arreglo.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("El arreglo 'Para' está vacío.");
                    }

                    // Agregar destinatarios en copia
                    if (ConCopia.Length > 0)
                    {
                        foreach (var item2 in ConCopia)
                        {
                            if (!string.IsNullOrEmpty(item2))
                            {
                                Console.WriteLine($"El correo copia para => {item2}");
                                mensaje.To.Add(item2);
                            }
                            else
                            {
                                Console.WriteLine("Elemento vacío o nulo en el arreglo.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("El arreglo 'ConCopia' está vacío.");
                    }

                    // Agregar adjuntos si hay
                    if (Adjuntos != null && Adjuntos.Length > 0)
                    {
                        foreach (var archivo in Adjuntos)
                        {
                            if (File.Exists(archivo))
                            {
                                Attachment adjunto = new(archivo);
                                mensaje.Attachments.Add(adjunto);
                            }
                        }
                    }

                    //Agregar adjuntos generados al momento
                    if (adjuntoPDF != null)
                        mensaje.Attachments.Add(adjuntoPDF);

                    // Enviar el correo
                    await clienteSmtp.SendMailAsync(mensaje);


                    return "Correo enviado satisfactoriamente";
                }
                else
                {
                    return "Cuerpo de mensaje vacío";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                return "Error al enviar el correo";
            }
        }

        /// <summary>
        /// Reemplazo de plantilla
        /// </summary>
        /// <param name="html"></param>
        /// <param name="replacements"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>

        public static string ReplaceHtmlPlaceholders(string html, Dictionary<string, string> replacements)
        {
            if (string.IsNullOrWhiteSpace(html))
                throw new ArgumentException("El texto HTML no puede estar vacío o ser nulo.", nameof(html));

            if (replacements == null || replacements.Count == 0)
                return html; // Si no hay reemplazos, retorna el HTML original.

            foreach (var replacement in replacements)
            {
                // Reemplaza todas las ocurrencias del índice con su valor correspondiente.
                html = html.Replace(replacement.Key, replacement.Value);
            }

            return html;
        }



        public async Task<Attachment> GenerarPdf(string htmlContent)
        {
            await new BrowserFetcher().DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);

            var pdfStream = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.Letter,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "1cm",
                    Right = "1cm",
                    Bottom = "1cm",
                    Left = "1cm"
                }
            });

            await browser.CloseAsync();

            pdfStream.Position = 0;
            var pdfBytes = new MemoryStream();
            await pdfStream.CopyToAsync(pdfBytes);
            pdfBytes.Position = 0;


            using var ms = new MemoryStream(pdfBytes.ToArray());
            Attachment adjunto = new(ms, "OficioNotificacion.pdf", MediaTypeNames.Application.Octet);
            return adjunto;
        }

        public async Task<string> NotificacionReporteSivigila(long idReporteSivigila, string entidadId, string userId)
        {

            string resultado = await this.OperacionReporteSivigila(idReporteSivigila, entidadId, "1");

            var resp = await this.GuardarOperacionSivigila(idReporteSivigila, entidadId, userId);



            return resultado;
        }

        public async Task<string> GuardarOperacionSivigila(long idReporteSivigila, string entidadId, string userId)
        {
            DateTime fechaActual = DateTime.Now;

            var seguimiento = new NotificacionReporteSivigila()
            {
                IdReporteSivigila = (int)idReporteSivigila,
                EntidadId = entidadId,
                TotalEnvios = 1,
                CreatedByUserId = userId,
                DateCreated = fechaActual

            };
            _context.NotificacionReporteSivigila.Add(seguimiento);
            await _context.SaveChangesAsync();

            return "Exito";
        }

        public async Task RevisarYEnviarNotificaciones()
        {
            var hoy = DateTime.UtcNow.Date;

            var registros = await _context.NotificacionReporteSivigila
                .Where(n => n.TotalEnvios < (byte)3) // Solo registros con menos de 3 envíos
                .ToListAsync();

            foreach (var registro in registros)
            {
                int siguienteEnvio = (byte)(registro.TotalEnvios + 1);
                DateTime? fechaEsperadaEnvio = registro.DateCreated?.AddDays(siguienteEnvio * 3).Date;

                if (hoy >= fechaEsperadaEnvio) // Si ya pasaron los días necesarios
                {
                    try
                    {
                        string resultado = await OperacionReporteSivigila(registro.IdReporteSivigila ?? 0, registro.EntidadId, siguienteEnvio.ToString());

                        if (resultado == "Exito")
                        {
                            registro.TotalEnvios = (byte)siguienteEnvio;
                            _context.NotificacionReporteSivigila.Update(registro);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error procesando reporte {registro.IdReporteSivigila}: {ex.Message}");
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> OperacionReporteSivigila(long idReporteSivigila, string entidadId, string numeroNotificacion)
        {

            /*
           * paso 1: obtener los datos del reporte sivigila y del destino del correo
           */
            List<ContactoEntidades> dataContactos = _context.ContactoEntidades
                .Where(x => x.EntidadId == entidadId)
                .ToList();
            var jsonData = JsonSerializer.Serialize(dataContactos, new JsonSerializerOptions { WriteIndented = true });
            /*Console.WriteLine(jsonData);
            Debug.WriteLine("Data: " + string.Join(", ", dataContacto));*/

            ReportesSIVIGILA dataReporte = _context.ReportesSIVIGILA.FirstOrDefault(x => x.Id == idReporteSivigila);
            int departamentoId = string.IsNullOrEmpty(dataReporte.DepartamentoProcedenciaId) ? 0 : Convert.ToInt32(dataReporte.DepartamentoProcedenciaId);

            BiStgDepartamento dataDepartamento = _context.BiStgDepartamento
                .FirstOrDefault(x => x.COD_DPTO == departamentoId);

            BiStgMunicipio dataMunicipio = _context.BiStgMunicipio.FirstOrDefault(x => x.COD_MUNICIPIO == dataReporte.MunicipioProcedenciaId && x.COD_DPTO == departamentoId);



            /*var jsonData2 = JsonSerializer.Serialize(dataReporte, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonData2);*/

            /*
             * paso 2:  consultar los datos de la plantilla 
             */
            long idPlantilla = 3;
            PlantillaCorreo? plantillaCorreo = (from p in _context.PlantillaCorreos
                                                where p.Id == idPlantilla
                                                select p).FirstOrDefault();

            /*var jsonData3 = JsonSerializer.Serialize(plantillaCorreo, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonData3);*/


            string Asunto = plantillaCorreo.Asunto.Replace("{{}}", numeroNotificacion); ;
            string Body = plantillaCorreo.Mensaje;


            //Procedemos a generar la lista de contactos

            var contactos = JsonSerializer.Deserialize<List<ContactoEntidades>>(jsonData);

            // Paso 2: Extraer los correos electrónicos y construir el array
            string[] Para = contactos.Where(c => !string.IsNullOrEmpty(c.Email))
                                     .Select(c => c.Email)
                                     .ToArray();

            //var jsonData4 = JsonSerializer.Serialize(Para, new JsonSerializerOptions { WriteIndented = true });

            //Console.WriteLine("Para "+ jsonData4);
            string[] ConCopia = Array.Empty<string>();

            //TODO: tener presente el tema de los adjuntos creando el pdf ( pendiente definicion )

            string membreteUrl = "data:image/png;base64,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";
            string footerUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB0AAAAaCAYAAABLlle3AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAuSURBVEhL7c2hAQAgDMCw/f80+PmAaWRN53wwO7zQlGpKNaWaUk2pplRTqil1AWqtvN5BFtSJAAAAAElFTkSuQmCC";
            string firma = "data:image/png;base64,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"; ;


            string contentHtml = contentidoPDFNotificacion();

            string laFecha = CalcularEdad(dataReporte!.FechaNacimiento!).ToString() ?? "";
            string diagnostico = (bool)dataReporte.TieneDiagnostico! ? "Sí" : "No";

            // Fecha actual
            DateTime fechaActual = DateTime.Now;

            // Cultura en español
            CultureInfo culturaEspañol = new CultureInfo("es-ES");

            // Formato deseado
            string fechaFormateada = fechaActual.ToString("dd 'de' MMMM 'de' yyyy", culturaEspañol);

            string lugar = dataDepartamento.NOM_DPTO + " " + dataMunicipio.Municipio;

            // Reemplazar los valores
            Dictionary<string, string> replacements = new Dictionary<string, string>
            {
                { "{nombre}", dataReporte.PrimerApellido+" "+dataReporte.SegundoApellido+" "+dataReporte.PrimerNombre+" "+dataReporte.SegundoApellido },
                { "{identificacion}", dataReporte.NumeroIdentificacion! },
                { "{edad}", laFecha },
                { "{diagnostico}", diagnostico },
                { "{fecha}", fechaFormateada },
                { "{lugar}", lugar }
            };

            string reemplazo = ReplaceHtmlPlaceholders(contentHtml, replacements);

            var AdjuntoPDF = await CreateNotificationPdfAsync(membreteUrl, footerUrl, reemplazo);


            //TODO: Adjuntar los que se generan por formulario

            // Obtener los archivos usando las funciones existentes
            var evidenciaDiagnostico = await _reportesSIVIGILARepo.EvidenciaDiagnostico(idReporteSivigila);
            var evidenciaParentesco = await _reportesSIVIGILARepo.EvidenciaParentesco(idReporteSivigila);

            List<string> adjuntosList = new List<string>();
            List<string> tempFiles = new List<string>();

            // Verificar si existen los archivos y guardarlos en archivos temporales
            if (evidenciaDiagnostico != null && evidenciaDiagnostico.FileBytes != null)
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), evidenciaDiagnostico.FileName);
                File.WriteAllBytes(tempFilePath, evidenciaDiagnostico.FileBytes);
                adjuntosList.Add(tempFilePath);
                tempFiles.Add(tempFilePath);
            }

            if (evidenciaParentesco != null && evidenciaParentesco.FileBytes != null)
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), evidenciaParentesco.FileName);
                File.WriteAllBytes(tempFilePath, evidenciaParentesco.FileBytes);
                adjuntosList.Add(tempFilePath);
                tempFiles.Add(tempFilePath);
            }

            // Convertir la lista a un array para pasarlo a la función de correo
            string[] Adjuntos = adjuntosList.ToArray();

            string resultado = await this.PlantillaCorreo(Para, ConCopia!, Asunto, Body, Adjuntos, AdjuntoPDF);

            return resultado;
        }

        public static int CalcularEdad(DateTime? fechaNacimiento)
        {

            if (!fechaNacimiento.HasValue)
            {
                // Retorna null para indicar que no hay edad calculable
                return 0;
            }

            // Fecha actual
            DateTime fechaActual = DateTime.Now;

            // Calcula los años preliminares
            int años = fechaActual.Year - fechaNacimiento.Value.Year;

            // Ajusta si el cumpleaños aún no ha ocurrido este año
            if (fechaActual < fechaNacimiento.Value.AddYears(años))
            {
                años--;
            }

            return años;
        }


        public async Task<string> NotificacionSolicitudSeguimiento(string cuidadorId, long nnaId, string agenteSeguimientoId, string userId)
        {


            string resultado = await this.OperacionNotificacionSolicitudSeguimiento(cuidadorId, nnaId, agenteSeguimientoId);


            DateTime fechaActual = DateTime.Now;

            var seguimiento = new NotificacionSolicitudSeguimiento()
            {
                CuidadorId = cuidadorId,
                NNAId = nnaId,
                AgenteSeguimientoId = agenteSeguimientoId,
                TotalEnvios = 1,
                CreatedByUserId = userId,
                DateCreated = fechaActual

            };
            _context.NotificacionSolicitudSeguimiento.Add(seguimiento);
            await _context.SaveChangesAsync();

            return resultado;





        }

        public async Task<string> OperacionNotificacionSolicitudSeguimiento(string cuidadorId, long nnaId, string agenteSeguimientoId)
        {
            //Consultar los datos del usuario y del nna
            //Console.WriteLine("data " + cuidadorId + " - " + nnaId + " - "+agenteSeguimientoId);
            ApplicationUser dataUsuario = (from p in _context.Users
                                           where p.Id == cuidadorId
                                           select p).FirstOrDefault();

            /*var jsonData2 = JsonSerializer.Serialize(dataUsuario, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonData2);*/

            var dataNna = (from p in _context.NNAs
                           where p.Id == nnaId
                           select new
                           {
                               Id = p.Id,
                               TipoIdentificacionId = p.TipoIdentificacionId,
                               NumeroIdentificacion = p.NumeroIdentificacion,
                               Nombre = p.PrimerNombre +
                                        (string.IsNullOrEmpty(p.SegundoNombre) ? "" : " " + p.SegundoNombre) +
                                        " " + p.PrimerApellido +
                                        (string.IsNullOrEmpty(p.SegundoApellido) ? "" : " " + p.SegundoApellido)
                           }).FirstOrDefault();

            /*var jsonData3 = JsonSerializer.Serialize(dataNna, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonData3);*/

            var diccionario = new Dictionary<string, string>
            {
                { "{{Id}}", dataNna.Id.ToString() },
                { "{{nnaTipoIdentificacion}}", dataNna.TipoIdentificacionId?.ToString() ?? string.Empty },
                { "{{nnaNumeroIdentificacion}}", dataNna.NumeroIdentificacion?.ToString() ?? string.Empty },
                { "{{nnaNombre}}", dataNna?.Nombre },
                { "{{cuidadorNombre}}", dataUsuario?.FullName }
            };


            long idPlantilla = 4;
            PlantillaCorreo? plantillaCorreo = (from p in _context.PlantillaCorreos
                                                where p.Id == idPlantilla
                                                select p).FirstOrDefault();

            var jsonData4 = JsonSerializer.Serialize(plantillaCorreo, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(jsonData4);

            string reemplazo = ReplaceHtmlPlaceholders(plantillaCorreo?.Mensaje, diccionario);
            //string reemplazo = plantillaCorreo.Mensaje;

            string Asunto = plantillaCorreo.Asunto;
            string Body = reemplazo;

            ApplicationUser dataAgente = (from p in _context.Users
                                          where p.Id == agenteSeguimientoId
                                          select p).FirstOrDefault();



            string[] Para = string.IsNullOrEmpty(dataAgente?.Email) ? Array.Empty<string>() : new string[] { dataAgente.Email };
            string[] ConCopia = Array.Empty<string>();
            string[] Adjuntos = Array.Empty<string>();

            string resultado = await this.PlantillaCorreo(Para, ConCopia!, Asunto, Body, Adjuntos);

            return resultado;
        }

        public string GenerateHtmlTemplate(string membreteUrl, string footerUrl, string content)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Notificación de Alerta</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        line-height: 1.6;
                    }}
                    .membrete {{
                        width: 100%;
                        text-align: right;
                        position: fixed;
                        top: 0;
                        left: 0;
                    }}
                    .membrete img {{
                        width: 80px;
                        max-height: 92px;
                    }}
                    .footer {{
                        width: 100%;
                        text-align: center;
                        position: fixed;
                        bottom: 0;
                        left: 0;
                    }}
                    .footer img {{
                        width: 100%;
                        max-height: 10px;
                    }}
                    .content {{
                        padding: 20px;
                        margin-top: 50px; /* Espacio para el membrete */
                        margin-bottom: 50px; /* Espacio para el pie de página */
                    }}
                    @media print {{
                        .membrete {{
                            position: fixed;
                            top: 0;
                        }}
                        .footer {{
                            position: fixed;
                            bottom: 0;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='membrete'>
                    <img src='{membreteUrl}' alt='Membrete' />
                </div>
                <div class='content'>
                    {content}
                </div>
                <div class='footer'>
                    <img src='{footerUrl}' alt='Footer' />
                </div>
            </body>
            </html>";
        }


        public async Task<MemoryStream> GeneratePdfAsync(string htmlContent)
        {
            await new BrowserFetcher().DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);

            var pdfStream = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.Letter,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "1cm",
                    Right = "1cm",
                    Bottom = "1cm",
                    Left = "1cm"
                }
            });

            await browser.CloseAsync();

            var pdfBytes = new MemoryStream();
            await pdfStream.CopyToAsync(pdfBytes);
            pdfBytes.Position = 0;
            return pdfBytes;
        }

        public Attachment CreatePdfAttachment(MemoryStream pdfBytes, string fileName)
        {
            pdfBytes.Position = 0;
            return new Attachment(pdfBytes, fileName, MediaTypeNames.Application.Pdf);
        }

        public async Task<Attachment> CreateNotificationPdfAsync(string membreteUrl, string footerUrl, string contentHtml)
        {
            // Generar el HTML
            string htmlContent = GenerateHtmlTemplate(membreteUrl, footerUrl, contentHtml);

            // Generar el PDF
            var pdfBytes = await GeneratePdfAsync(htmlContent);

            // Crear el adjunto
            return CreatePdfAttachment(pdfBytes, "Notificacion.pdf");
        }

        public string contentidoPDFNotificacion()
        {
            var cadena = $@"
            <div class=""content"">
                <p>Bogotá, {{fecha}}</p>
    
                <p>Respetadas EPS / ET.</p>

                <p>Ciudad: <span class=""highlight"">{{lugar}}</span></p>

                <p><strong>Asunto:</strong> Notificación de alerta por barrera de acceso en la atención integral de los niños, niñas y adolescentes con cáncer infantil</p>
    
                <p>
                    La ley 1388 de 2010 y ley 2026 del 2020 definió las medidas para hacer efectiva la 
                    protección del derecho fundamental a la salud de los menores de 18 años con 
                    diagnóstico o presunción de cáncer, y declaró su atención integral como prioritaria, 
                    garantizando el acceso efectivo a los servicios de salud oncopediátrica y el 
                    fortalecimiento al apoyo social que recibe esta población.
                </p>

                <p>
                    En este sentido, el Ministerio de Salud y Protección Social, en el marco de la Estrategia 
                    de Seguimiento Nacional de Cáncer Infantil, se ha identificado la siguiente barrera:
                </p>

                <p><strong>Oportunidad - 2.D:</strong> No realizar la notificación obligatoria de los eventos de cáncer 
                infantil contemplados en el Sivigila.</p>

                <p>
                    Desde la Estrategia de Seguimiento Nacional de Cáncer Infantil se ha identificado que 
                    el niño, niña o adolescente no ha sido reportado en la base de datos de SIVIGILA, por 
                    lo que se solicita realizar el reporte conforme a lo establecido en la Ley 1388 de 2010. 
                    A continuación, se describen los datos del paciente:
                </p>

                <ul>
                    <li><strong>Nombre del paciente: {{nombre}}</strong></li>
                    <li><strong>Identificación: {{identificacion}}</strong></li>
                    <li><strong>Edad: {{edad}}</strong></li>
                    <li><strong>Diagnóstico: {{diagnostico}}</strong></li>
                    <li><strong>Nombre del acudiente: {{nombre_acudiente}}</strong></li>
                    <li><strong>Teléfono de contacto: {{telefono}}</strong></li>
                </ul>
                <br><br>
                <br><br>
                <br><br>
                <br><br>
                <p>
                    Teniendo en cuenta que los niños, niñas y adolescentes con sospecha o diagnóstico de 
                    cáncer infantil son de especial protección de acuerdo al artículo 13 de la Constitución 
                    Política de Colombia y la circular 04 de 2014 expedida por la Superintendencia Nacional 
                    de Salud, la cual define las instrucciones para la garantía de la atención de esta población, 
                    solicitamos de manera respetuosa que se solucione la barrera de acceso identificada 
                    y se remita la gestión realizada del caso al correo <a href=""mailto:psnc@minsalud.gov.co"">psnc@minsalud.gov.co</a> 
                    en un término de 5 días hábiles de recibida esta notificación.
                </p>

                <p>Cordialmente,</p>

                <p><strong>Estrategia De Seguimiento Nacional de Cáncer</strong></p>
                <p>Centro de Contacto Ciudadano</p>
                <p>Teléfono: 601 330 5043</p>
                <p><a href=""http://www.minsalud.gov.co/"">www.minsalud.gov.co</a></p>

                <p style=""font-size: small; color: gray;"">
                    Antes de imprimir este mensaje piense bien si es necesario hacerlo.
                </p>
            </div>
            ";

            return cadena;
        }

        public async Task EnviarNotificacionAsignacionAgentes(UserDto[] agentes, List<UsuarioAsignado> asignados)
        {
            var subject = "[SECÁNI] NUEVAS ASIGNACIONES";
            var html = @"
                <!DOCTYPE html>
                <html>
                <body>
                    <p style='font-family: Arial, sans-serif; font-style: italic;'>
                        Se le han asignado los siguientes casos:
                    </p>
                    <ul style='font-family: Arial, sans-serif;'>
                        {listaAsignados}
                    </ul>
                </body>
                </html>
            ";

            var listaAsignados = string.Join("", asignados.Select(a => $@"
                <li>
                    <strong>{a.DocumentoNNA}</strong> - {a.NombreNNA}
                </li>"));

            var usuariosNotificar = asignados
                .Select(a => a.UsuarioId)
                .Distinct()
                .ToArray();

            foreach (var usuario in usuariosNotificar)
            {
                var agente = agentes.FirstOrDefault(u => u.Id == usuario);
                if (agente != null)
                {
                    html = html.Replace("{listaAsignados}", listaAsignados);
                    var body = html;

                    var result = PlantillaCorreo([agente.Email ?? ""], [], subject, body, []);
                }
            }
        }

        public async Task EnviarNotificacionAsignacionCoordinadores(string[] para, List<UsuarioAsignado> asignados, List<UsuarioAsignado> reagendados, List<UsuarioAsignado> reasignados)
        {
            var fecha = DateTime.Now.ToString("dd/MM/yyyy");
            var hora = DateTime.Now.ToString("HH:mm:ss");
            var total = asignados.Count + reasignados.Count + reagendados.Count;
            var asignadosCount = asignados.Count;
            var reasignadosCount = reasignados.Count;
            var reagendadosCount = reagendados.Count;

            var html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        table {width: 100%;
                            border-collapse: collapse;
                        }
                        th, td {border: 1px solid black;
                            padding: 8px;
                            text-align: center;
                        }
                        th {background-color: #f2f2f2;
                        }
                    </style>
                </head>
                <body>
                    <table>
		                <tr>
                            <th colspan='6'>Reporte de asignaciones – {fecha}</th>
                        </tr>
                        <tr>
                            <th>Fecha</th>
                            <th>Hora</th>
                            <th>Seguimientos asignados</th>
                            <th>Seguimientos reasignados</th>
                            <th>Seguimientos reagendados</th>
                            <th>Total</th>
                        </tr>
                        <tr>
                            <td>{fecha}</td>
                            <td>{hora}</td>
                            <td>{asignados}</td>
                            <td>{reasignados}</td>
                            <td>{reagendados}</td>
                            <td>{total}</td>
                        </tr>
                    </table>
                    </br>
                    <p> </p>
                    </br>
                    <table>
		                <tr>
                            <th colspan='7'>Detalle reporte de asignaciones</th>
                        </tr>
                        <tr>
                            <th>ID</th>
                            <th>Fecha</th>
                            <th>NNA</th>
                            <th>No. de Seguimiento</th>
                            <th>Estado</th>
                            <th>Criterio</th>
                            <th>Agente de seguimiento activo</th>
                        </tr>
                        {asignadosRows}
                    </table>
                </body>
                </html>
            ";

            var asignadosRows = string.Join("", asignados.Select(a => $@"
                <tr>
                    <td>{a.Id}</td>
                    <td>{fecha}</td>
                    <td>{a.NombreNNA}</td>
                    <td>{a.SeguimientoId}</td>
                    <td>Asignado</td>
                    <td>Registro por primera vez</td>
                    <td>{a.NombreUsuario}</td>
                </tr>"));

            var reasignadosRows = string.Join("", reasignados.Select(a => $@"
                <tr>
                    <td>{a.Id}</td>
                    <td>{fecha}</td>
                    <td>{a.NombreNNA}</td>
                    <td>{a.SeguimientoId}</td>
                    <td>Reasignado</td>
                    <td>Agente inactivo</td>
                    <td>{a.NombreUsuario}</td>
                </tr>"));

            var reagendadosRows = string.Join("", reagendados.Select(a => $@"
                <tr>
                    <td>{a.Id}</td>
                    <td>{fecha}</td>
                    <td>{a.NombreNNA}</td>
                    <td>{a.SeguimientoId}</td>
                    <td>Reagendado</td>
                    <td>{a.Criterio}</td>
                    <td>{a.NombreUsuario}</td>
                </tr>"));

            html = html.Replace("{asignadosRows}", asignadosRows);

            var replacements = new Dictionary<string, string>
            {
                { "{fecha}", fecha },
                { "{hora}", hora },
                { "{asignados}", asignadosCount.ToString() },
                { "{reasignados}", reasignadosCount.ToString() },
                { "{reagendados}", reagendadosCount.ToString() },
                { "{total}", total.ToString() },
            };

            var content = ReplaceHtmlPlaceholders(html, replacements);
            var subject = $"REPORTE DE ASIGNACIONES - {fecha} {hora}";
            var body = content;

            var result = await PlantillaCorreo(para, [], subject, body, []);
        }

        public async Task<RespuestaResponse<bool>> ValidarNotificacion(int id)
        {
            try
            {
                var notificacion = await (from p in _context.NotificacionesEntidad
                                          where p.Id == id
                                          select p).FirstOrDefaultAsync();

                if (notificacion == null)
                    return new() { Estado = false, Descripcion = "No se encontró la notificación a responder" };

                var fechaActual = DateTime.Now;
                var fechaCreacion = notificacion.DateCreated ?? DateTime.Now;
                var fechaLimite = fechaCreacion.AddDays(8);
                if (fechaActual > fechaLimite)
                    return new() { Estado = false, Descripcion = "La notificación ya no se puede responder" };

                return new() { Estado = true, Descripcion = "La notificación se puede responder" };
            }
            catch (Exception ex)
            {
                return new() { Estado = false, Descripcion = ex.Message };
            }
        }
    }
}
