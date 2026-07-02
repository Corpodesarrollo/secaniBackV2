using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Modelos.Identity;
using Core.Request;
using Core.Services.StorageService;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SISPRO.TRV.Entity;
using static Core.Common.Estructuras;

namespace Infra.Repositorios
{
    public class ReportesSIVIGILARepo : IReportesSIVIGILARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<ReportesSIVIGILA> _repository;
        private readonly IStorageService _storageService;
        private readonly Lazy<INotificacionRepo> _notificacionRepo;
        private readonly ISeguimientoRepo _seguimientoRepo;


        public ReportesSIVIGILARepo(ApplicationDbContext context, IStorageService storageService, IServiceProvider serviceProvider, ISeguimientoRepo seguimientoRepo)
        {
            _context = context;
            GenericRepository<ReportesSIVIGILA> repository = new(_context);
            _repository = repository;
            _storageService = storageService;
            _notificacionRepo = new Lazy<INotificacionRepo>(() => serviceProvider.GetRequiredService<INotificacionRepo>());
            _seguimientoRepo = seguimientoRepo;

        }

        public async Task<IEnumerable<ReportesSIVIGILADto>> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repository.GetAllAsync(cancellationToken);
                var dtos = GenericMapper.MapList<ReportesSIVIGILA, ReportesSIVIGILADto>(result);
                return dtos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ReportesSIVIGILADto>> GetAllPorEnviar(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _repository.FindAllAsync(x => x.Estado == 0);
                var dtos = GenericMapper.MapList<ReportesSIVIGILA, ReportesSIVIGILADto>(result);
                return dtos;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ReportesSIVIGILADto?> GetById(long id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);
            return dto;
        }

        public async Task<(bool, ReportesSIVIGILA)> AddAsync(ReportesSIVIGILADto data, User user)
        {
            try
            {
                var entity = GenericMapper.Map<ReportesSIVIGILADto, ReportesSIVIGILA>(data);
                entity.Estado = 0;
                entity.Id = 0;
                // CreatedByUserId: priorizar UsuarioId enviado por front (Cuidador logueado).
                // Sin SISPRO en EC2 el user de claims es vacio - sin esto quedaba "Sistema" hardcode.
                var creatorId = !string.IsNullOrEmpty(data.UsuarioId) ? data.UsuarioId
                                : !string.IsNullOrEmpty(user?.Alias) ? user.Alias
                                : "Sistema";
                entity.CreatedByUserId = creatorId;
                var (success, response) = await _repository.AddAsync(entity);

                if (success)
                {
                    //await CrearSeguimiento(data, user);
                    if (data.EvidenciaDiagnostico != null)
                        await _storageService.UploadFileAsync(data.EvidenciaDiagnostico?.FileBytes, $"RS-EvidenciaDiagnostico-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaDiagnostico.Extension}", true);

                    if (data.EvidenciaParentesco != null)
                        await _storageService.UploadFileAsync(data.EvidenciaParentesco?.FileBytes, $"RS-EvidenciaParentesco-{entity.Id}-{data.NumeroIdentificacion}{data.EvidenciaParentesco.Extension}", true);

                    // BUG-LZ-spinner-sivigila: aislar RevisarYEnviarNotificaciones para que no rompa
                    // el POST. El reporte SIVIGILA ya fue creado y subido a storage; si falta la tabla
                    // NotificacionReporteSivigila o algun envio externo cuelga, no debe revertirse el
                    // resultado al usuario. Procesamiento de notificaciones es best-effort.
                    try
                    {
                        await _notificacionRepo.Value.RevisarYEnviarNotificaciones();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"WARN RevisarYEnviarNotificaciones fallo (POST SIVIGILA continua): {ex.Message}");
                    }
                }

                return (success, response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // BUG-LZ-040: variante con motivo para que controller exponga mensaje util al frontend.
        public async Task<(bool success, string? message)> CrearSeguimientoDetallado(SeguimientoDto data, User user)
        {
            try
            {
                var nna = await _context.NNAs.FirstOrDefaultAsync(x => x.TipoIdentificacionId == data.TipoIdentificacion && x.NumeroIdentificacion == data.NumeroIdentificacion);
                if (nna == null)
                    return (false, "El NNA no se encuentra registrado en SECANI. Antes de solicitar un seguimiento, debe crearlo en el modulo Histórico NNA.");

                var contactos = await _context.ContactoNNAs.Where(x => x.NNAId == nna.Id && x.Cuidador).ToListAsync();
                var contacto = contactos.FirstOrDefault(x => x.Cuidador) ?? contactos.FirstOrDefault();

                var usuario = await (from ua in _context.UsuarioAsignados
                                     join u in _context.Users on ua.UsuarioId equals u.Id
                                     join s in _context.Seguimientos on ua.SeguimientoId equals s.Id
                                     join n in _context.NNAs on s.NNAId equals n.Id
                                     where n.Id == nna.Id && u.Activo == true
                                     orderby ua.FechaAsignacion descending
                                     select u).FirstOrDefaultAsync();

                var usuarioOrigen = await _context.Users.FirstOrDefaultAsync(x => x.Alias == "CC3216549872");

                // BUG-LZ-069: Alias CC3216549872 hardcoded no existe en BD QA. Si NNA sin agente previo
                // y no existe ese usuario solicitante, retornaba error "No hay agente disponible".
                // Fallback: primer agente activo con rol "Agentes de seguimiento" (id 14CDDEA5...).
                // BUG-LZ-082: el fallback solo corria si usuario y usuarioOrigen eran null. Cuando el
                // NNA YA tenia agente (usuario != null) pero el alias hardcoded no existia,
                // usuarioOrigen quedaba null y mas abajo CrearNotificacion hacia userOrigen.Id -> NRE
                // ("Object reference not set..."). Ahora resolvemos usuarioOrigen siempre que sea null.
                if (usuarioOrigen == null)
                {
                    const string ROL_AGENTE_ID = "14CDDEA5-FA06-4331-8359-036E101C5046";
                    usuarioOrigen = await (from u in _context.Users
                                           join ur in _context.UserRoles on u.Id equals ur.UserId
                                           where ur.RoleId == ROL_AGENTE_ID && u.Activo == true
                                           select u).FirstOrDefaultAsync();
                }

                if (usuario == null && usuarioOrigen == null)
                    return (false, "No hay agente disponible para asignar este seguimiento. Contacte al administrador.");

                var usuarioInicialId = usuario?.Id ?? usuarioOrigen!.Id;

                // BUG-LZ-083: el solicitante es el usuario autenticado (Cuidador) que pide el
                // seguimiento. GetSeguimientosCuidador lista por SolicitanteId == AspNetUsers.Id.
                // Preferimos el data.UsuarioId que el front envia (xUser.id del Cuidador logueado)
                // porque el lookup por Alias del JWT no siempre resuelve bien en QA -> caia al agente.
                string? solicitanteId = null;
                if (!string.IsNullOrWhiteSpace(data.UsuarioId))
                {
                    var existe = await _context.Users.AnyAsync(u => u.Id == data.UsuarioId);
                    if (existe) solicitanteId = data.UsuarioId;
                }
                if (solicitanteId == null)
                {
                    var solicitante = await _context.Users.FirstOrDefaultAsync(u => u.Alias == user.Alias);
                    solicitanteId = solicitante?.Id;
                }

                var seguimiento = new SetSeguimientoRequest()
                {
                    NNAId = nna.Id,
                    FechaSeguimiento = DateTime.Now,
                    EstadoId = 1,
                    ContactoNNAId = contacto != null ? contacto.Id : 0,
                    UsuarioId = usuarioInicialId,
                    SolicitanteId = solicitanteId ?? usuarioOrigen?.Id,
                    FechaSolicitud = DateTime.Now,
                    TieneDiagnosticos = true,
                    UltimaActuacionFecha = DateTime.Now
                };

                var seguimientoId = await _seguimientoRepo.SetSeguimiento(seguimiento);
                var asignanciones = await _seguimientoRepo.AsignacionAutomatica(
                    (seguimientoId, $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}", nna.NumeroIdentificacion ?? ""), usuario);

                if (asignanciones.Count == 0)
                    return (false, "No fue posible asignar el seguimiento automaticamente. Contacte al administrador.");

                // BUG-LZ-082: guard defensivo. Si aun no hay solicitante (sin agentes activos con rol),
                // se crea el seguimiento pero se omite la notificacion en vez de tirar NRE.
                if (usuarioOrigen != null)
                    await CrearNotificacion(new()
                    {
                        TipoIdentificacionId = nna.TipoIdentificacionId,
                        NumeroIdentificacion = nna.NumeroIdentificacion
                    }, usuarioOrigen, asignanciones[0]);

                return seguimientoId > 0
                    ? (true, null)
                    : (false, "No se pudo crear el seguimiento.");
            }
            catch (Exception ex)
            {
                return (false, $"Error interno: {ex.Message}");
            }
        }

        public async Task<bool> CrearSeguimiento(SeguimientoDto data, User user)
        {
            try
            {
                var nna = await _context.NNAs.FirstOrDefaultAsync(x => x.TipoIdentificacionId == data.TipoIdentificacion && x.NumeroIdentificacion == data.NumeroIdentificacion);
                if (nna == null)
                    return false;

                var contactos = await _context.ContactoNNAs.Where(x => x.NNAId == nna.Id && x.Cuidador).ToListAsync();
                var contacto = contactos.FirstOrDefault(x => x.Cuidador) ?? contactos.FirstOrDefault();

                var fechaValidar = DateTime.Now.Date.AddDays(1);
                var diaSemana = (int)fechaValidar.DayOfWeek;

                var usuario = await (from ua in _context.UsuarioAsignados
                                     join u in _context.Users on ua.UsuarioId equals u.Id
                                     join s in _context.Seguimientos on ua.SeguimientoId equals s.Id
                                     join n in _context.NNAs on s.NNAId equals n.Id
                                     where n.Id == nna.Id && u.Activo == true
                                     orderby ua.FechaAsignacion descending
                                     select u).FirstOrDefaultAsync();

                var usuarioOrigen = await _context.Users.FirstOrDefaultAsync(x => x.Alias == "CC3216549872");
                //var usuarioOrigen = await _context.Users.FirstOrDefaultAsync(x => x.Alias == user.Alias);

                // BUG-LZ-029: si el NNA nunca tuvo asignacion previa la query devuelve null y
                // antes hacia NRE en usuario.Id silencioso por el try/catch. Fallback al usuario
                // origen (Sistema/Solicitante) para que SetSeguimiento no falle; AsignacionAutomatica
                // mas abajo se encarga de re-asignar al agente correspondiente.
                // BUG-LZ-069: Alias hardcoded inexistente. Fallback primer agente activo.
                if (usuario == null && usuarioOrigen == null)
                {
                    const string ROL_AGENTE_ID = "14CDDEA5-FA06-4331-8359-036E101C5046";
                    usuarioOrigen = await (from u in _context.Users
                                           join ur in _context.UserRoles on u.Id equals ur.UserId
                                           where ur.RoleId == ROL_AGENTE_ID && u.Activo == true
                                           select u).FirstOrDefaultAsync();
                }

                if (usuario == null && usuarioOrigen == null)
                {
                    return false;
                }
                var usuarioInicialId = usuario?.Id ?? usuarioOrigen!.Id;

                // BUG-LZ-083: SolicitanteId = Id del usuario autenticado (Cuidador) que solicita.
                var solicitante = await _context.Users.FirstOrDefaultAsync(u => u.Alias == user.Alias);

                var seguimiento = new SetSeguimientoRequest()
                {
                    NNAId = nna.Id,
                    FechaSeguimiento = DateTime.Now,
                    EstadoId = 1, // Estado inicial
                    ContactoNNAId = contacto != null ? contacto.Id : 0,
                    UsuarioId = usuarioInicialId,
                    SolicitanteId = solicitante?.Id ?? usuarioOrigen?.Id,
                    FechaSolicitud = DateTime.Now,
                    TieneDiagnosticos = true,
                    UltimaActuacionFecha = DateTime.Now
                };

                var seguimientoId = await _seguimientoRepo.SetSeguimiento(seguimiento);
                var asignanciones = await _seguimientoRepo.AsignacionAutomatica(
                    (seguimientoId, $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}", nna.NumeroIdentificacion ?? ""), usuario);

                if (asignanciones.Count == 0)
                    return false;

                await CrearNotificacion(new()
                {
                    TipoIdentificacionId = nna.TipoIdentificacionId,
                    NumeroIdentificacion = nna.NumeroIdentificacion
                }, usuarioOrigen, asignanciones[0]);

                return seguimientoId > 0;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        async Task<bool> CrearNotificacion(ReportesSIVIGILADto data, ApplicationUser userOrigen, UsuarioAsignado asignado)
        {
            var usuario = await _context.Users.FirstOrDefaultAsync(x => x.Id == asignado.UsuarioId);
            if (usuario == null)
                return false;

            var nna = await _context.NNAs.FirstOrDefaultAsync(x => x.TipoIdentificacionId == data.TipoIdentificacionId && x.NumeroIdentificacion == data.NumeroIdentificacion);
            if (nna == null)
                return false;

            var noti = await _notificacionRepo.Value.SetNotificacion(new()
            {
                IdSeguimiento = nna.Id,
                AgenteOrigen = userOrigen.Id,
                AgenteDestino = usuario.Id,
                Administrador = true,
                TipoNotificacion = TipoNotificacion.AsignacionSolicitudesCuidadores,
                TextoNotificacion = $"El -RolOrigen- -NombresOrigen- ha solicitado un seguimiento sobre el caso No. {nna.Id:000000} y este le fue asignado al -RolDestino- -NombresDestino-"
            });

            var noti2 = await _notificacionRepo.Value.SetNotificacion(new()
            {
                IdSeguimiento = nna.Id,
                AgenteOrigen = userOrigen.Id,
                AgenteDestino = usuario.Id,
                Administrador = false,
                TipoNotificacion = TipoNotificacion.AsignacionSolicitudesCuidadores,
                TextoNotificacion = $"El -RolOrigen- -NombresOrigen- ha solicitado un seguimiento sobre el caso No. {nna.Id:000000}"
            });

            return true;
        }

        public async Task<UploadFileRequest?> EvidenciaDiagnostico(long id)
        {

            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);

            // Construcción del nombre del archivo
            string fileName = $"RS-EvidenciaDiagnostico-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaDiagnostico?.Extension}";

            // Intentar descargar el archivo
            var evidenciaDiagnostico = await _storageService.DownloadFileAsync(fileName);

            // Validar si el archivo no existe o está vacío
            if (evidenciaDiagnostico == null || evidenciaDiagnostico.Length == 0)
                return null;

            return new UploadFileRequest
            {
                FileBytes = evidenciaDiagnostico,
                FileName = fileName
            };
        }

        public async Task<UploadFileRequest?> EvidenciaParentesco(long id)
        {

            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return null;

            var dto = GenericMapper.Map<ReportesSIVIGILA, ReportesSIVIGILADto>(result);

            string fileName = $"RS-EvidenciaParentesco-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaParentesco?.Extension}";

            var evidenciaParentesco = await _storageService.DownloadFileAsync(fileName);

            // Validar si el archivo no existe o está vacío
            if (evidenciaParentesco == null || evidenciaParentesco.Length == 0)
                return null;

            return new()
            {
                FileBytes = evidenciaParentesco,
                FileName = $"RS-EvidenciaParentesco-{dto.Id}-{dto.NumeroIdentificacion}{dto.EvidenciaParentesco?.Extension}"
            };
        }

        public async Task<(bool, ReportesSIVIGILA)> UpdateAsync(ReportesSIVIGILADto data)
        {
            var entity = GenericMapper.Map<ReportesSIVIGILADto, ReportesSIVIGILA>(data);
            var (success, response) = await _repository.UpdateAsync(entity);
            return (success, response);
        }
    }
}
