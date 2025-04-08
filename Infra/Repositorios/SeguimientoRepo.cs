using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Core.Utilities;
using iText.Html2pdf;
using iText.Kernel.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class SeguimientoRepo(ApplicationDbContext context, IWebHostEnvironment env, TablaParametricaService tablaParametricaService) : ISeguimientoRepo
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        private IQueryable<SeguimientoDto> GetSelect(string id)
        {
            var query = from s in _context.Seguimientos
                        join n in _context.NNAs on s.NNAId equals n.Id
                        where s.UsuarioId == id
                        group s by s.NNAId into g
                        select new { id = g.Max(x => x.Id) };

            return from q in query
                   join s in _context.Seguimientos on q.id equals s.Id
                   join n in _context.NNAs on s.NNAId equals n.Id
                   join e in _context.TPEstadoNNA on n.estadoId equals e.Id
                   select new SeguimientoDto()
                   {
                       Id = s.Id,
                       NoCaso = s.NNAId,
                       PrimerNombre = n.PrimerNombre,
                       SegundoNombre = n.SegundoNombre,
                       PrimerApellido = n.PrimerApellido,
                       SegundoApellido = n.SegundoApellido,
                       FechaNotificacion = n.FechaNotificacionSIVIGILA,
                       FechaSeguimiento = s.FechaSeguimiento,
                       Estado = new TPEstadoNNADto()
                       {
                           Nombre = e.Nombre,
                           Descripcion = e.Descripcion,
                           ColorBG = e.ColorBG,
                           ColorText = e.ColorText
                       },
                       AsuntoUltimaActuacion = s.UltimaActuacionAsunto,
                       FechaUltimaActuacion = s.UltimaActuacionFecha,
                       Alertas = (from als in _context.AlertaSeguimientos
                                  join a in _context.Alertas on als.AlertaId equals a.Id
                                  join ea in _context.TPEstadoAlerta on als.EstadoId equals ea.Id
                                  join sca in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                  where als.SeguimientoId == s.Id
                                  select new Core.DTOs.AlertaSeguimientoDto { Nombre = sca.CategoriaAlertaId + "." + sca.Indicador, Id = ea.Id }).ToList()
                   };
        }

        public async Task<List<SeguimientoDto>> GetAllByIdUser(string id, int filtro)
        {
            try
            {
                var query = GetSelect(id);
                var result = await query.ToListAsync();

                if (filtro == 1) //hoy
                    return result.Where(x => x.FechaSeguimiento?.Date == DateTime.Now.Date).ToList();

                else if (filtro == 2) //con alerta
                    return result.Where(x => x.Alertas.Count > 0).ToList();

                else if (filtro == 3) //Todos
                    return result;

                else if (filtro == 4) //Solicitados por Cuidador
                    return result.Where(x => x.AsuntoUltimaActuacion?.ToLower() == "solicitado por cuidador").ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SeguimientoCntFiltrosDto> GetCntSeguimiento(string id)
        {
            try
            {
                var query = GetSelect(id);
                var result = await query.ToListAsync();

                return new SeguimientoCntFiltrosDto
                {
                    Todos = result.Count,
                    Hoy = result.Where(x => x.FechaSeguimiento?.Date == DateTime.Now.Date).Count(),
                    ConAlerta = result.Where(x => x.Alertas.Count > 0).Count(),
                    SolicitadosPorCuidador = result.Count(x => x.AsuntoUltimaActuacion?.ToLower() == "solicitado por cuidador")
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<long> GetCntSeguimientoByNNA(long id)
        {
            try
            {
                return await _context.Seguimientos.CountAsync(x => x.NNAId == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SeguimientoDatosNNADto?> SeguimientoNNA(long id)
        {
            try
            {
                var result = await (from n in _context.NNAs
                                    join d in _context.CIE10s on n.DiagnosticoId equals d.Id into diag
                                    from diagnostico in diag.DefaultIfEmpty()
                                    where n.Id == id
                                    select new SeguimientoDatosNNADto
                                    {
                                        IdNNA = n.Id,
                                        NombreCompleto = string.Join(" ", n.PrimerNombre, n.SegundoNombre, n.PrimerApellido, n.SegundoApellido),
                                        Diagnostico = diagnostico.Nombre,
                                        FechaNacimiento = n.FechaNacimiento,
                                        FechaIngresoEstrategia = n.FechaIngresoEstrategia,
                                        FechaInicioSeguimiento = _context.Seguimientos.Where(s => s.NNAId == id).Select(s => s.FechaSeguimiento).FirstOrDefault(),
                                        SeguimientosRealizados = _context.Seguimientos.Count(s => s.NNAId == id)
                                    }).FirstOrDefaultAsync();

                if (result != null)
                {
                    result.Edad = Funciones.CalcularEdad(result.FechaNacimiento);
                    result.TiempoTranscurrido = Funciones.CalcularTiempoTrascurrido(result.FechaInicioSeguimiento!.Value);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Seguimiento? GetById(long id)
        {
            return _context.Seguimientos?.FirstOrDefault(s => s.Id == id);
        }

        public List<GetSeguimientoResponse> RepoSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoResponse> response = (from un in _context.Seguimientos
                                                     join nna in _context.NNAs on un.NNAId equals nna.Id
                                                     join ua in _context.UsuarioAsignados on new { un.Id, un.UsuarioId } equals new { Id = ua.SeguimientoId, ua.UsuarioId }
                                                     join alerta in _context.AlertaSeguimientos on un.Id equals alerta.SeguimientoId into alertaGroup
                                                     from subAlerta in alertaGroup.DefaultIfEmpty()
                                                     where ua.UsuarioId == UsuarioId
                                                           && ua.FechaAsignacion >= FechaInicial
                                                           && ua.FechaAsignacion <= FechaFinal
                                                        && un.EstadoId != 3 && ua.Activo
                                                     group subAlerta by new
                                                     {
                                                         un.Id,
                                                         un.NNAId,
                                                         ua.FechaAsignacion, // Usando FechaAsignacion como FechaSeguimiento
                                                         un.EstadoId,
                                                         un.ContactoNNAId,
                                                         un.Telefono,
                                                         un.UsuarioId,
                                                         un.SolicitanteId,
                                                         un.FechaSolicitud,
                                                         un.TieneDiagnosticos,
                                                         un.ObservacionesSolicitante,
                                                         nna.PrimerNombre,
                                                         nna.SegundoNombre,
                                                         nna.PrimerApellido,
                                                         nna.SegundoApellido,
                                                         nna.FechaNotificacionSIVIGILA
                                                     } into g
                                                     select new GetSeguimientoResponse()
                                                     {
                                                         Id = g.Key.Id,
                                                         NNAId = g.Key.NNAId,
                                                         FechaSeguimiento = g.Key.FechaAsignacion, // Ajustando la fecha de seguimiento
                                                         EstadoId = g.Key.EstadoId,
                                                         ContactoNNAId = g.Key.ContactoNNAId,
                                                         Telefono = g.Key.Telefono,
                                                         UsuarioId = g.Key.UsuarioId,
                                                         SolicitanteId = g.Key.SolicitanteId,
                                                         FechaSolicitud = g.Key.FechaSolicitud ?? new(),
                                                         TieneDiagnosticos = g.Key.TieneDiagnosticos ?? false,
                                                         ObservacionesSolicitante = g.Key.ObservacionesSolicitante,
                                                         PrimerNombre = g.Key.PrimerNombre,
                                                         SegundoNombre = g.Key.SegundoNombre,
                                                         PrimerApellido = g.Key.PrimerApellido,
                                                         SegundoApellido = g.Key.SegundoApellido,
                                                         FechaNotificacionSIVIGILA = g.Key.FechaNotificacionSIVIGILA,
                                                         CantidadAlertas = g.Count(subAlerta => subAlerta != null)
                                                     }).ToList();



            return response;
        }

        public int RepoSeguimientoActualizacionFecha(PutSeguimientoActualizacionFechaRequest request)
        {

            var usuarioAsignado = _context.UsuarioAsignados.FirstOrDefault(s => s.SeguimientoId == request.Id);

            if (usuarioAsignado == null)
            {
                return -1;
            }

            usuarioAsignado.FechaAsignacion = request.FechaSeguimiento;

            _context.SaveChanges();
            return 1;
        }

        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            var UsuarioOriginal = _context.UsuarioAsignados.FirstOrDefault(s => s.SeguimientoId == request.Id);

            if (UsuarioOriginal == null)
            {
                return -1;
            }


            DateTime hoy = DateTime.Now;
            if (UsuarioOriginal.FechaAsignacion < hoy)
            {
                return -2;
            }

            // Actualizar el EstadoId a falso
            UsuarioOriginal.Activo = false;
            UsuarioOriginal.Observaciones = request.ObservacionesSolicitante!;

            // Guardar los cambios en el seguimiento original
            _context.SaveChanges();

            try
            {

                var nuevoUsuarioAsignado = new UsuarioAsignado
                {

                    UsuarioId = request.UsuarioId,
                    SeguimientoId = UsuarioOriginal.SeguimientoId,
                    FechaAsignacion = UsuarioOriginal.FechaAsignacion,
                    Activo = true,
                    DateCreated = DateTime.Now,
                    CreatedByUserId = UsuarioOriginal.CreatedByUserId,
                    Observaciones = "Creado por Reasignación"

                };

                _context.UsuarioAsignados.Add(nuevoUsuarioAsignado);
                _context.SaveChanges();
            }
            catch (Exception)
            {

                return -3;
            }

            return 1;
        }

        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal, string UsuarioId)
        {
            var festivos = from un in _context.TPFestivos
                           where un.Festivo.Date >= FechaInicial.Date
                                 && un.Festivo.Date <= FechaFinal.Date
                           select new GetSeguimientoFestivoResponse
                           {
                               Festivo = un.Festivo.Date,
                           };

            var ausencias = from a in _context.Ausencias
                            where a.FechaAusencia.Date >= FechaInicial.Date
                                  && a.FechaAusencia.Date <= FechaFinal.Date
                                  && a.UsuarioId == UsuarioId
                            select new GetSeguimientoFestivoResponse
                            {
                                Festivo = a.FechaAusencia.Date, // Coincide el formato de solo fecha
                            };

            // Hacemos la unión de los resultados de ambas consultas
            var unionResult = festivos
                             .Union(ausencias) // Une las dos listas
                             .OrderBy(x => x.Festivo) // Ordenamos por fecha si es necesario
                             .ToList();

            return unionResult;
        }


        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId)
        {
            List<GetSeguimientoHorarioAgenteResponse> response = (from un in _context.HorarioLaboralAgente
                                                                  where
                                                                  un.UserId == UsuarioId


                                                                  select new GetSeguimientoHorarioAgenteResponse()
                                                                  {

                                                                      Dia = un.Dia,
                                                                      HoraEntrada = un.HoraEntrada,
                                                                      HoraSalida = un.HoraSalida,

                                                                  }).ToList();



            return response;
        }

        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId)
        {
            var response = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            join u in _context.Users on ur.UserId equals u.Id
                            where r.Name.Contains("Agentes de seguimiento")
                            && u.Id != UsuarioId
                            select new GetSeguimientoAgentesResponse
                            {
                                Id = u.Id,
                                FullName = u.FullName!
                            }).ToList();

            return response;
        }

        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request)
        {
            Seguimiento? seguimiento = (from seg in _context.Seguimientos
                                        where seg.Id == request.IdSeguimiento
                                        select seg).FirstOrDefault();

            if (seguimiento != null)
            {
                seguimiento.EstadoId = request.IdEstado;
                _context.Seguimientos.Update(seguimiento);
                _context.SaveChanges();
            }
        }

        public async Task<SeguimientoDto[]> GetSeguimientosByNNA(int idNNA)
        {
            var query = from s in _context.Seguimientos
                        join n in _context.NNAs on s.NNAId equals n.Id
                        join e in _context.TPEstadoNNA on n.estadoId equals e.Id
                        where n.Id == idNNA
                        select new SeguimientoDto()
                        {
                            Id = s.Id,
                            NoCaso = s.NNAId,
                            PrimerNombre = n.PrimerNombre,
                            SegundoNombre = n.SegundoNombre,
                            PrimerApellido = n.PrimerApellido,
                            SegundoApellido = n.SegundoApellido,
                            FechaNotificacion = s.FechaSolicitud,
                            FechaSeguimiento = s.UltimaActuacionFecha,
                            Observaciones = s.ObservacionAgente,
                            EntidadAlerta = string.Join(", ", (from als in _context.AlertaSeguimientos
                                                               join na in _context.NotificacionesEntidad on als.Id equals na.AlertaSeguimientoId
                                                               join en in _context.Entidades on na.EntidadId equals en.Id
                                                               where als.SeguimientoId == s.Id
                                                               select en.Nombre).ToArray()),
                            //FechaRespuesta = (from als in _context.RespuestaAlerta
                            //                  join na in _context.NotificacionesEntidad on als.Id equals na.AlertaSeguimientoId
                            //                  join en in _context.Entidades on na.EntidadId equals en.Id
                            //                  where als.SeguimientoId == s.Id
                            //                  select en.Nombre).ToArray(),
                            Estado = new TPEstadoNNADto()
                            {
                                Nombre = e.Nombre,
                                Descripcion = e.Descripcion,
                                ColorBG = e.ColorBG,
                                ColorText = e.ColorText
                            },
                            AsuntoUltimaActuacion = s.UltimaActuacionAsunto,
                            FechaUltimaActuacion = s.UltimaActuacionFecha,
                            Alertas = (from als in _context.AlertaSeguimientos
                                       join a in _context.Alertas on als.AlertaId equals a.Id
                                       join ea in _context.TPEstadoAlerta on als.EstadoId equals ea.Id
                                       join sca in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                       where als.SeguimientoId == s.Id
                                       select new AlertaSeguimientoDto { Nombre = sca.CategoriaAlertaId + "." + sca.Indicador, Id = ea.Id }).ToList()
                        };

            return await query.ToArrayAsync();
        }

        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA)
        {
            List<SeguimientoNNAResponse> seguimientos = (from seg in _context.Seguimientos
                                                         join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                         where seg.NNAId == idNNA
                                                         select new SeguimientoNNAResponse()
                                                         {
                                                             FechaNotificacion = seg.FechaSolicitud ?? new(),
                                                             FechaSeguimiento = seg.UltimaActuacionFecha,
                                                             IdSeguimiento = seg.Id,
                                                             Asunto = seg.UltimaActuacionAsunto,
                                                             Observacion = seg.ObservacionAgente,
                                                             FechaInicioSeguimiento = seg.FechaSeguimiento,
                                                             NNA = new NNAResponse()
                                                             {
                                                                 Id = nna.Id,
                                                                 FechaNacimiento = nna.FechaNacimiento,
                                                                 NombreCompleto = string.Join("", nna.PrimerNombre, " ", nna.SegundoNombre, " ", nna.PrimerApellido, " ", nna.SegundoApellido),
                                                                 Diagnostico = "",
                                                                 IdEstado = nna.estadoId
                                                             }
                                                         }).ToList();

            List<AlertaSeguimientoResponse>? alertas;
            foreach (SeguimientoNNAResponse seg in seguimientos)
            {
                alertas = (from alert in _context.AlertaSeguimientos
                           join al in _context.Alertas on alert.AlertaId equals al.Id
                           join subal in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals subal.Id
                           join catal in _context.TPCategoriaAlerta on subal.CategoriaAlertaId equals catal.Id
                           join ea in _context.TPEstadoAlerta on alert.EstadoId equals ea.Id
                           join sca in _context.TPSubCategoriaAlerta on al.SubcategoriaId equals sca.Id
                           where alert.SeguimientoId == seg.IdSeguimiento
                           select new AlertaSeguimientoResponse()
                           {
                               AlertaId = alert.AlertaId,
                               EstadoId = alert.EstadoId,
                               Observaciones = alert.Observaciones,
                               SeguimientoId = alert.SeguimientoId,
                               UltimaFechaSeguimiento = (DateTime)alert.UltimaFechaSeguimiento,
                               NombreAlerta = sca.CategoriaAlertaId + "." + sca.Indicador,
                               SubcategoriaAlerta = subal.Indicador + ". " + subal.SubCategoriaAlerta,
                               CategoriaAlerta = catal.Id + ". " + catal.Nombre
                           }).ToList();

                seg.alertasSeguimientos = alertas;
            }

            return seguimientos;
        }

        public int RepoSeguimientoRechazo(PutSeguimientoRechazoRequest request)
        {

            var seguimiento = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimiento == null)
            {
                return -1;
            }
            seguimiento.EstadoId = 3;
            seguimiento.NombreRechazo = request.NombreRechazo;
            seguimiento.ParentescoRechazo = request.ParentescoRechazo;
            seguimiento.RazonesRechazo = request.RazonesRechazo;

            var contactonna = _context.ContactoNNAs.FirstOrDefault(s => s.Id == seguimiento.ContactoNNAId);
            if (contactonna == null)
            {
                return -1;
            }

            contactonna.TelefnosInactivos = contactonna.TelefnosInactivos + ' ' + contactonna.Telefonos;
            contactonna.Telefonos = "";

            _context.SaveChanges();
            return 1;
        }

        public GetNNaParcialResponse GetNNaById(long id)
        {


            GetNNaParcialResponse? response = (from u in _context.NNAs
                                               where u.Id != id
                                               select new GetNNaParcialResponse
                                               {
                                                   Id = u.Id,
                                                   PrimerNombre = u.PrimerNombre,
                                                   SegundoNombre = u.SegundoNombre,
                                                   PrimerApellido = u.PrimerApellido,
                                                   SegundoApellido = u.SegundoApellido,
                                                   FechaNotificacionSIVIGILA = u.FechaNotificacionSIVIGILA

                                               }).FirstOrDefault();

            return response;


        }

        public async Task<string> SetSeguimiento(SetSeguimientoRequest request)
        {
            try
            {
                var ultimaFechaSeguimiento = await _context.Seguimientos.Where(s => s.NNAId == request.NNAId).OrderByDescending(x => x.FechaSeguimiento).Select(s => s.FechaSeguimiento).FirstOrDefaultAsync();

                var seguimiento = new Seguimiento()
                {
                    NNAId = request.NNAId,
                    FechaSeguimiento = request.FechaSeguimiento,
                    EstadoId = request.EstadoId,
                    ContactoNNAId = request.ContactoNNAId,
                    Telefono = request.Telefono,
                    UsuarioId = request.UsuarioId,
                    SolicitanteId = request.SolicitanteId,
                    FechaSolicitud = request.FechaSolicitud,
                    TieneDiagnosticos = request.TieneDiagnosticos,
                    ObservacionesSolicitante = request.ObservacionesSolicitante,
                    ObservacionAgente = request.ObservacionAgente,
                    UltimaActuacionAsunto = request.UltimaActuacionAsunto,
                    UltimaActuacionFecha = request.UltimaActuacionFecha,
                    NombreRechazo = request.NombreRechazo,
                    ParentescoRechazo = request.ParentescoRechazo,
                    RazonesRechazo = request.RazonesRechazo,
                    CreatedByUserId = "1"
                };
                _context.Seguimientos.Add(seguimiento);
                await _context.SaveChangesAsync();

                if (request.Alertas != null)
                {
                    foreach (var item in request.Alertas)
                    {
                        var alerta = new Alerta()
                        {
                            SubcategoriaId = item,
                            Descripcion = "Alerta generada por seguimiento",
                            Alias = 'S',
                            CreatedByUserId = "1",
                            DateCreated = DateTime.Now
                        };
                        _context.Alertas.Add(alerta);
                        await _context.SaveChangesAsync();

                        var alertaSeguimiento = new AlertaSeguimiento()
                        {
                            CreatedByUserId = "1",
                            DateCreated = DateTime.Now,
                            EstadoId = 1,
                            AlertaId = alerta.Id,
                            SeguimientoId = seguimiento.Id,
                            Observaciones = "Alerta generada por seguimiento",
                            UltimaFechaSeguimiento = ultimaFechaSeguimiento
                        };
                        _context.AlertaSeguimientos.Add(alertaSeguimiento);
                        await _context.SaveChangesAsync();
                    }
                }

                if (request.alertasPendientes != null)
                {
                    foreach (var item in request.alertasPendientes)
                    {
                        var alertaSeguimiento = await _context.AlertaSeguimientos.FirstOrDefaultAsync(x => x.AlertaId == item.Id);
                        if (alertaSeguimiento != null)
                        {
                            alertaSeguimiento.EstadoId = item.Resuelta ?? false ? 4 : 3;
                            alertaSeguimiento.Observaciones = item.Resuelta ?? false ? "Alerta resuelta en seguimiento" : "Alerta sin resolver en seguimiento";
                            alertaSeguimiento.UltimaFechaSeguimiento = DateTime.Now;
                            _context.AlertaSeguimientos.Update(alertaSeguimiento);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return "Segumiento almacenado correctamente";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int RepoSeguimientoRechazo(PutSeguimientoRechazoRequest request)
        {

            var seguimiento = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimiento == null)
            {
                return -1;
            }
            seguimiento.EstadoId = 3;
            seguimiento.NombreRechazo = request.NombreRechazo;
            seguimiento.ParentescoRechazo = request.ParentescoRechazo;
            seguimiento.RazonesRechazo = request.RazonesRechazo;

            var contactonna = _context.ContactoNNAs.FirstOrDefault(s => s.Id == seguimiento.ContactoNNAId);
            if (contactonna == null)
            {
                return -1;
            }

            contactonna.TelefnosInactivos = contactonna.TelefnosInactivos + ' ' + contactonna.Telefonos;
            contactonna.Telefonos = "";

            _context.SaveChanges();
            return 1;
        }

        public async Task<List<UsuarioAsignado>> AsignacionAutomatica()
        {
            var revisoresAusentes = new List<UsuariosHorariosDto>();
            var seguimientosAsignados = new List<UsuarioAsignado>();

            var seguimientosNoAsignados = await CargarSeguimientos();

            var fecha = DateTime.Now.Date;
            var revisores = await CargarRevisores(fecha);

            while (seguimientosNoAsignados.Count > 0 && revisores.Count > 0)
            {
                while (seguimientosNoAsignados.Count > 0 && revisores.Count > 0)
                {
                    var revisor = revisores.Select(x => x).OrderByDescending(x => x.CantidadSeguimientosDisponibles).FirstOrDefault();

                    //validar que la fecha no es dia festivo
                    var festivos = await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo == fecha);
                    if (festivos != null)
                        break;

                    //validamos los seguimientos asignados al revisor en la fecha
                    var seguimientosAsignadosFecha = await _context.UsuarioAsignados.Where(x => x.UsuarioId == revisor.UserId && x.FechaAsignacion.Value.Date == fecha.Date).ToListAsync();
                    if (seguimientosAsignadosFecha.Count > 0)
                        revisor.CantidadSeguimientosDisponibles -= seguimientosAsignadosFecha.Count;

                    //valida si el revisor tiene seguimeintos disponibles por asignar
                    if (revisor.CantidadSeguimientosDisponibles <= 0)
                    {
                        revisores.Remove(revisor);
                        continue;
                    }

                    //validar lista de seguimientos no asignados
                    if (seguimientosNoAsignados.Count == 0)
                        break;

                    //el revisor entra a las horaentrada y sale a la horasalida. sedebe asignar el seguimiento en un rango de 640 segundos,
                    //si el seguimiento se cruza con otro se debe aumentar 640 segundos  y volver a verificar hasta lograr agendar el seguimiento
                    var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);

                    var seguimiento = seguimientosNoAsignados[0];

                    //asignar seguimiento al revisor
                    var usuarioAsignado = new UsuarioAsignado
                    {
                        Activo = true,
                        DateCreated = DateTime.Now,
                        FechaAsignacion = fechaAsignacion,
                        Observaciones = "Asignación automática",
                        SeguimientoId = seguimiento,
                        UsuarioId = revisor.UserId
                    };

                    _context.UsuarioAsignados.Add(usuarioAsignado);
                    await _context.SaveChangesAsync();

                    seguimientosAsignados.Add(usuarioAsignado);

                    seguimientosNoAsignados.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                    revisor.CantidadSeguimientosDisponibles--;
                }

                fecha = fecha.AddDays(1);
                revisores = await CargarRevisores(fecha);
            }

            return seguimientosAsignados;
        }

        private static DateTime BuscarEspacioHorario(DateTime fecha, UsuariosHorariosDto revisor, List<UsuarioAsignado> seguimientosAsignadosFecha)
        {
            var fechaAsignacion = fecha.Date + revisor.HoraEntrada.GetValueOrDefault();
            var fechaSalida = fecha.Date + revisor.HoraSalida.GetValueOrDefault();
            var fechaEncontrada = false;
            while (!fechaEncontrada && fechaAsignacion < fechaSalida)
            {
                var seguimientosAsignadosFechaRango = seguimientosAsignadosFecha.Where(x => x.FechaAsignacion > fechaAsignacion && x.FechaAsignacion <= fechaAsignacion.AddSeconds(640)).FirstOrDefault();
                if (seguimientosAsignadosFechaRango != null)
                {
                    fechaAsignacion = fechaAsignacion.AddSeconds(640);
                    continue;
                }
                else
                    fechaEncontrada = true;
            }

            return fechaAsignacion;
        }

        public async Task<List<UsuarioAsignado>> AsignacionAutomaticaReagendar()
        {
            var fecha = DateTime.Now;

            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            var revisores = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   join u in _context.Users on ur.UserId equals u.Id
                                   join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                                   join a in _context.Ausencias on new { a = u.Id, b = fecha } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                                   from aus in a.DefaultIfEmpty()
                                   where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Fecha == fecha && aus != null
                                   select new UsuariosHorariosDto
                                   {
                                       UserId = u.Id,
                                       Fecha = h.Fecha,
                                       HoraEntrada = h.HoraEntrada,
                                       HoraSalida = h.HoraSalida
                                   }).ToListAsync();

            var seguimientosReagendados = new List<UsuarioAsignado>();

            foreach (var revisor in revisores)
            {
                var seguimientos = from s in _context.Seguimientos
                                   join n in _context.NNAs on s.NNAId equals n.Id
                                   where s.UsuarioId == revisor.UserId
                                   group s by s.NNAId into g
                                   select new { id = g.Max(x => x.Id) };

                var seguimientosReagendamiento = await (from q in seguimientos
                                                        join seg in _context.Seguimientos on q.id equals seg.Id
                                                        join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId
                                                        where seg.FechaSeguimiento < fecha && ua.UsuarioId == revisor.UserId
                                                        select seg.Id).ToListAsync();

                while (seguimientosReagendamiento.Count > 0)
                {
                    fecha = fecha.Date.AddDays(1);

                    //validar que la fecha no es dia festivo
                    var festivos = await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo == fecha);
                    if (festivos != null)
                        continue;

                    //validamos los seguimientos asignados al revisor en la fecha
                    var seguimientosAsignadosFecha = await _context.UsuarioAsignados.Where(x => x.UsuarioId == revisor.UserId && x.FechaAsignacion == fecha).ToListAsync();
                    if (seguimientosAsignadosFecha.Count > 0)
                        revisor.CantidadSeguimientosDisponibles -= seguimientosAsignadosFecha.Count;

                    //valida si el revisor tiene seguimeintos disponibles por asignar
                    if (revisor.CantidadSeguimientosDisponibles <= 0)
                        continue;

                    var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);

                    var seguimiento = seguimientosReagendamiento[0];

                    //asignar seguimiento al revisor
                    var usuarioAsignado = new UsuarioAsignado
                    {
                        Activo = true,
                        DateCreated = DateTime.Now,
                        FechaAsignacion = fechaAsignacion,
                        Observaciones = "Reagendamiento automático",
                        SeguimientoId = seguimiento,
                        UsuarioId = revisor.UserId
                    };

                    _context.UsuarioAsignados.Add(usuarioAsignado);
                    await _context.SaveChangesAsync();

                    seguimientosReagendados.Add(usuarioAsignado);
                    seguimientosReagendamiento.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                }
            }

            return seguimientosReagendados;
        }

        public async Task<List<UsuarioAsignado>> AsignacionAutomaticaReasignacion()
        {
            var fecha = DateTime.Now.Date;
            var seguimientosAsignados = new List<UsuarioAsignado>();

            var seguimientos = from s in _context.Seguimientos
                               join n in _context.NNAs on s.NNAId equals n.Id
                               group s by s.NNAId into g
                               select new { id = g.Max(x => x.Id) };

            var seguimientosReasignacion = await (from q in seguimientos
                                                  join seg in _context.Seguimientos on q.id equals seg.Id
                                                  join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId
                                                  join u in _context.Users on ua.UsuarioId equals u.Id
                                                  where ua.FechaAsignacion == fecha && u.Activo == false
                                                  select seg.Id).ToListAsync();

            var revisores = await CargarRevisoresReasignacion(fecha);

            while (seguimientosReasignacion.Count > 0 && revisores.Count > 0)
            {
                while (seguimientosReasignacion.Count > 0 && revisores.Count > 0)
                {
                    var revisor = revisores.Select(x => x).OrderByDescending(x => x.CantidadSeguimientosDisponibles).FirstOrDefault();

                    //validar que la fecha no es dia festivo
                    var festivos = await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo == fecha);
                    if (festivos != null)
                        break;

                    //validamos los seguimientos asignados al revisor en la fecha
                    var seguimientosAsignadosFecha = await _context.UsuarioAsignados.Where(x => x.UsuarioId == revisor.UserId && x.FechaAsignacion == fecha).ToListAsync();
                    if (seguimientosAsignadosFecha.Count > 0)
                        revisor.CantidadSeguimientosDisponibles -= seguimientosAsignadosFecha.Count;

                    //valida si el revisor tiene seguimeintos disponibles por asignar
                    if (revisor.CantidadSeguimientosDisponibles <= 0)
                    {
                        revisores.Remove(revisor);
                        continue;
                    }

                    //validar lista de seguimientos no asignados
                    if (seguimientosReasignacion.Count == 0)
                        break;

                    var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);

                    var seguimiento = seguimientosReasignacion[0];

                    //asignar seguimiento al revisor
                    var usuarioAsignado = new UsuarioAsignado
                    {
                        Activo = true,
                        DateCreated = DateTime.Now,
                        FechaAsignacion = fechaAsignacion,
                        Observaciones = "Asignación automática",
                        SeguimientoId = seguimiento,
                        UsuarioId = revisor.UserId
                    };

                    _context.UsuarioAsignados.Add(usuarioAsignado);
                    await _context.SaveChangesAsync();

                    seguimientosAsignados.Add(usuarioAsignado);

                    seguimientosReasignacion.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                    revisor.CantidadSeguimientosDisponibles--;
                }

                fecha = fecha.AddDays(1);
                revisores = await CargarRevisoresReasignacion(fecha);
            }

            return seguimientosAsignados;
        }

        private async Task<List<UsuariosHorariosDto>> CargarRevisoresReasignacion(DateTime fecha)
        {
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fecha } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Fecha == fecha && aus == null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).ToListAsync();
        }

        private async Task<List<long>> CargarSeguimientos()
        {
            return await (from seg in _context.Seguimientos
                          join nna in _context.NNAs on seg.NNAId equals nna.Id
                          join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId into ua
                          from uas in ua.DefaultIfEmpty()
                          where nna.estadoId == 15 && uas == null
                          select seg.Id).ToListAsync();
        }

        private async Task<List<UsuariosHorariosDto>> CargarRevisores(DateTime fecha)
        {
            var fechaValidar = fecha;

            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fechaValidar } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Fecha == fecha && aus == null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).ToListAsync();
        }

        public string CrearPlantillaCorreo(CrearPlantillaCorreoRequest request)
        {
            try
            {
                PlantillaCorreo? plantillaCorreo = (from p in _context.PlantillaCorreos
                                                    where p.Id == request.Id
                                                    select p).FirstOrDefault();

                if (plantillaCorreo == null)
                {
                    plantillaCorreo = new PlantillaCorreo()
                    {
                        Id = request.Id,
                        Asunto = request.Asunto,
                        Cierre = request.Cierre,
                        Estado = request.Estado,
                        FechaCreacion = DateTime.Now,
                        Firmante = request.Firmante,
                        Mensaje = request.Mensaje,
                        Nombre = request.Nombre,
                        TipoPlantilla = request.TipoPlantilla
                    };

                    _context.PlantillaCorreos.Add(plantillaCorreo);
                    _context.SaveChanges();

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        Transaccion = "Creacion",
                        Comentario = request.Comentario,
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = request.IdUsuario,
                        UsuarioRol = request.Rol
                    };

                    _context.HistoricosPlantilla.Add(historicoPlantilla);
                    _context.SaveChanges();

                    return "Plantilla creada exitosamente";
                }
                else
                {
                    plantillaCorreo.Asunto = request.Asunto;
                    plantillaCorreo.Cierre = request.Cierre;
                    plantillaCorreo.Estado = request.Estado;
                    plantillaCorreo.Firmante = request.Firmante;
                    plantillaCorreo.Mensaje = request.Mensaje;
                    plantillaCorreo.Nombre = request.Nombre;
                    plantillaCorreo.TipoPlantilla = request.TipoPlantilla;

                    _context.PlantillaCorreos.Update(plantillaCorreo);
                    _context.SaveChanges();

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        Transaccion = "Modificacion",
                        Comentario = request.Comentario,
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = request.IdUsuario,
                        UsuarioRol = request.Rol
                    };

                    _context.HistoricosPlantilla.Add(historicoPlantilla);
                    _context.SaveChanges();

                    return "Plantilla modificada exitosamente";
                }
            }
            catch (Exception)
            {
                return "Se presento un problema en el proceso";
            }

        }

        public string EliminarPlantillaCorreo(EliminarPlantillaCorreoRequest request)
        {
            try
            {
                PlantillaCorreo? plantillaCorreo = (from p in _context.PlantillaCorreos
                                                    where p.Id == request.Id
                                                    select p).FirstOrDefault();

                if (plantillaCorreo != null)
                {
                    _context.PlantillaCorreos.Remove(plantillaCorreo);
                    _context.SaveChanges();

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        Transaccion = "Eliminacion",
                        Comentario = request.Comentario,
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = request.IdUsuario,
                        UsuarioRol = request.Rol
                    };

                    _context.HistoricosPlantilla.Add(historicoPlantilla);
                    _context.SaveChanges();

                    return "Registro Eliminado Correctamente";
                }
                else
                {
                    return "El Id indicado no existe";
                }
            }
            catch (Exception)
            {
                return "Se presento un problema en el proceso";
            }
        }

        public List<ConsultarPlantillaResponse> ConsultarPlantillasCorreo()
        {
            List<ConsultarPlantillaResponse> response = (from p in _context.PlantillaCorreos
                                                         select new ConsultarPlantillaResponse()
                                                         {
                                                             Id = p.Id,
                                                             Asunto = p.Asunto,
                                                             Cierre = p.Cierre,
                                                             Estado = p.Estado,
                                                             FechaCreacion = p.FechaCreacion,
                                                             Firmante = p.Firmante,
                                                             Mensaje = p.Mensaje,
                                                             Nombre = p.Nombre,
                                                             TipoPlantilla = p.TipoPlantilla
                                                         }).ToList();

            return response;
        }

        public List<HistoricoPlantillaCorreoResponse> HistoricoPlantillaCorreo(string id)
        {
            List<HistoricoPlantillaCorreoResponse> response = (from h in _context.HistoricosPlantilla
                                                               select new HistoricoPlantillaCorreoResponse()
                                                               {
                                                                   Id = h.Id,
                                                                   FechaCreacion = h.FechaCreacion,
                                                                   Comentario = h.Comentario,
                                                                   Transaccion = h.Transaccion,
                                                                   UsuarioOrigen = h.UsuarioOrigen,
                                                                   UsuarioRol = h.UsuarioRol
                                                               }).ToList();
            return response;
        }

        public async Task<ExportarDetalleSeguimientoResponse> ExportarDetalleSeguimiento(long id)
        {
            ExportarDetalleSeguimientoResponse response = new();
            try
            {
                var query = from s in _context.Seguimientos
                            join n in _context.NNAs on s.NNAId equals n.Id
                            where n.Id == id
                            group s by s.NNAId into g
                            select new { id = g.Max(x => x.Id) };

                ExportarDetalleSeguimientoDto? seguimiento = (from q in query
                                                              join seg in _context.Seguimientos on q.id equals seg.Id
                                                              join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                              join c in _context.CIE10s on nna.DiagnosticoId equals c.Id
                                                              select new ExportarDetalleSeguimientoDto()
                                                              {
                                                                  Nombre = nna.PrimerNombre + " " + nna.SegundoApellido + " " + nna.PrimerApellido + " " + nna.SegundoApellido,
                                                                  FechaNacimiento = nna.FechaNacimiento,
                                                                  Diagnostico = c.Nombre,
                                                                  FechaSeguimiento = seg.FechaSeguimiento,
                                                                  Id = seg.Id,
                                                                  IdSexo = nna.SexoId,
                                                                  TipoIdentificacion = nna.TipoIdentificacionId,
                                                                  NumeroIdentificacion = nna.NumeroIdentificacion,
                                                                  PaisNacimiento = nna.PaisId,
                                                                  Etnia = nna.EtniaId,
                                                                  CiudadNacimiento = nna.MunicipioNacimientoId,
                                                                  OrigenReporte = nna.OrigenReporteId,
                                                                  EstadoIngreso = nna.estadoId,
                                                                  FechaIngreso = nna.FechaIngresoEstrategia,
                                                                  GrupoPoblacional = nna.GrupoPoblacionId,
                                                                  RegimenAfiliacion = nna.TipoRegimenSSId,
                                                                  Asegurador = nna.EPSId,
                                                                  Ips = nna.IPSId,
                                                                  IdNNA = nna.Id,
                                                                  razonesNoTratamiento = seg.RazonesRechazo,
                                                                  fechaConsulta = nna.FechaConsultaDiagnostico,
                                                                  fechaDiagnostico = nna.FechaDiagnostico,
                                                                  fechaInicioTratamiento = nna.FechaInicioTratamiento,
                                                                  IpsTratamiento = nna.IPSIdTratamiento.ToString(),
                                                                  tieneRecaidas = nna.Recaida,
                                                                  numeroRecaidas = nna.CantidadRecaidas,
                                                                  fechaUltimaRecaida = nna.FechaUltimaRecaida,
                                                                  departamentoResidencia = nna.ResidenciaOrigenMunicipioId,
                                                                  municipioResidencia = nna.ResidenciaOrigenMunicipioId,
                                                                  barrioResidencia = nna.ResidenciaOrigenBarrio,
                                                                  areaResidencia = nna.ResidenciaOrigenBarrio,
                                                                  direccionResidencia = nna.ResidenciaOrigenDireccion,
                                                                  estratoResidencia = nna.ResidenciaOrigenEstratoId,
                                                                  telefonoResidencia = nna.ResidenciaOrigenTelefono,
                                                                  requirioTrasladarse = nna.ResidenciaOrigenDireccion != nna.ResidenciaActualDireccion,
                                                                  departamentoResidenciaActual = nna.ResidenciaActualMunicipioId,
                                                                  municipioResidenciaActual = nna.ResidenciaActualMunicipioId,
                                                                  barrioResidenciaActual = nna.ResidenciaActualBarrio,
                                                                  direccionResidenciaActual = nna.ResidenciaActualDireccion,
                                                                  estratoResidenciaActual = nna.ResidenciaActualEstratoId,
                                                                  telefonoResidenciaActual = nna.ResidenciaActualTelefono,
                                                                  capacidadEconomicaTraslado = nna.TrasladoTieneCapacidadEconomica,
                                                                  apoyoTraslado = nna.TrasladoEAPBSuministroApoyo,
                                                                  apoyoOportuno = nna.TrasladosServiciosdeApoyoOportunos,
                                                                  coberturaServicioSocial = nna.TrasladosServiciosdeApoyoCobertura,
                                                                  nombreFundacion = nna.TrasladosNombreFundacion,
                                                                  apoyoFundacion = nna.TrasladosApoyoRecibidoxFundacion,
                                                                  tipoResidenciaActual = nna.ResidenciaActualCategoriaId,
                                                                  asumioCostosTraslado = nna.TrasladosQuienAsumioCostosTraslado,
                                                                  asumioCostosVivienda = nna.TrasladosQuienAsumioCostosVivienda,
                                                                  dificultadAutorizacionMedicamentos = nna.DifAutorizaciondeMedicamentos,
                                                                  dificultadEntregaMedicamentosLAP = nna.DifEntregaMedicamentosLAP,
                                                                  dificultadEntregaMedicamentosNoLAP = nna.DifEntregaMedicamentosNoLAP,
                                                                  dificultadAsignacionCitas = nna.DifAsignaciondeCitas,
                                                                  HanCobradoCopago = nna.DifHanCobradoCuotasoCopagos,
                                                                  AutorizacionProcedimiento = nna.DifAutorizacionProcedimientos,
                                                                  remisionEspecialista = nna.DifRemisionInstitucionesEspecializadas,
                                                                  MalaAtencionIps = nna.DifMalaAtencionIPS,
                                                                  cualIps = nna.DifMalaAtencionNombreIPSId,
                                                                  FallaMipres = nna.DifFallasenMIPRES,
                                                                  fallaConvenio = nna.DifFallaConvenioEAPBeIPSTratante,
                                                                  HaTrasladado = nna.TrasladosHaSidoTrasladadodeInstitucion,
                                                                  ips = nna.IPSId,
                                                                  haRecurridoAccionLegal = nna.TrasladosHaRecurridoAccionLegal,
                                                                  Motivo = nna.TrasladosMotivoAccionLegal,
                                                                  tipoRecurso = nna.TrasladosTipoAccionLegalId,
                                                                  haDejadoTratamiento = nna.TratamientoHaDejadodeAsistir,
                                                                  tiempoInasistenciaTratamiento = nna.TratamientoCuantoTiemposinAsistir,
                                                                  causaInasistencia = nna.TratamientoCausasInasistenciaId,
                                                                  estudiaActualmente = nna.TratamientoEstudiaActualmente,
                                                                  haDejadoColegio = nna.TratamientoHaDejadodeAsistirColegio,
                                                                  tiempoInasistenciaColegio = nna.TratamientoTiempoInasistenciaColegio,
                                                                  ipsClara = nna.TratamientoHaSidoInformadoClaramente
                                                              }).FirstOrDefault();

                seguimiento.Contactos = (from nna in _context.NNAs
                                         join cont in _context.ContactoNNAs on nna.Id equals cont.NNAId
                                         join p in _context.TPParentescos on cont.ParentescoId equals p.Id
                                         where nna.Id == seguimiento.IdNNA
                                         select new ExportarDetalleSeguimientoContactoDto()
                                         {
                                             CorreoElectronico = cont.Email,
                                             Nombre = cont.Nombres,
                                             Parentesco = p.Nombre,
                                             Telefono = cont.Telefonos
                                         }).ToList();

                seguimiento.CantidadSegumientos = (from seg in _context.Seguimientos
                                                   where seg.NNAId == seguimiento.IdNNA
                                                   select seg).Count();

                int edad = 0;
                if (seguimiento.FechaNacimiento != null)
                {
                    edad = DateTime.Now.Year - seguimiento.FechaNacimiento.Value.Year;

                    // Ajustar si la fecha de inicio no ha cumplido el mismo día/mes en el año final
                    if (DateTime.Now < seguimiento.FechaNacimiento.Value.AddYears(edad))
                    {
                        edad--;
                    }
                }

                var tipoIdentificacion = await tablaParametricaService.GetBynomTREF("APSTipoIdentificacion", CancellationToken.None);
                var paises = await tablaParametricaService.GetBynomTREF("Pais", CancellationToken.None);
                var GruposEtnicos = await tablaParametricaService.GetBynomTREF("GrupoEtnico", CancellationToken.None);
                var departamentos = await tablaParametricaService.GetBynomTREF("Departamento", CancellationToken.None);
                var municipios = await tablaParametricaService.GetBynomTREF("Municipio", CancellationToken.None);
                var zonas = await tablaParametricaService.GetBynomTREF("ZonaTerritorial", CancellationToken.None);
                var estratos = await tablaParametricaService.GetBynomTREF("EstratoSocioeconomico", CancellationToken.None);
                var tiposVivienda = await tablaParametricaService.GetBynomTREF("RIBATipoVivienda", CancellationToken.None);
                var tiposPoblacion = await tablaParametricaService.GetBynomTREF("LCETipoPoblacionEspecial", CancellationToken.None);
                var regimenes = await tablaParametricaService.GetBynomTREF("APSRegimenAfiliacion", CancellationToken.None);

                var estadoIngreso = await _context.tPEstadoIngresoEstrategia.FirstOrDefaultAsync(x => x.Id == seguimiento.EstadoIngreso);
                var origenReporte = await _context.TPOrigenReporte.FirstOrDefaultAsync(x => x.Id == seguimiento.OrigenReporte);
                var eps = await _context.TPEAPB.FirstOrDefaultAsync(x => x.Id == seguimiento.Asegurador);
                var ips = await _context.TPIPS.FirstOrDefaultAsync(x => x.Id == seguimiento.Ips);
                //var ipsTratamiento = await _context.TPIPS.FirstOrDefaultAsync(x => x.Id == seguimiento.IpsTratamiento);

                if (seguimiento != null)
                {
                    #region TEMPLATE
                    string htmlContent = @"
                    <!DOCTYPE html>
                    <html lang='es'>
                        <head>
                            <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                            <style type='text/css'>
                                * {
                                    margin: 0;
                                    padding: 0;
                                    text-indent: 0;
                                }

                                 .s1 {
                                    color: black;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: bold;
                                    text-decoration: none;
                                    font-size: 10pt;
                                }

                                 .s2 {
                                    color: black;
                                    font-family: Verdana,
                                        sans-serif;
                                    font-style: normal;
                                    font-weight: normal;
                                    text-decoration: none;
                                    font-size: 10pt;
                                }

                                 p {
                                    color:
                                        black;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: bold;
                                    text-decoration: none;
                                    font-size:
                                        8pt;
                                    margin: 0pt;
                                }

                                 .s4 {
                                    color: black;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight:
                                        normal;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 .s5 {
                                    color: black;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: bold;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 h1 {
                                    color: #0D0D0D;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: bold;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 .s6 {
                                    color: #0D0D0D;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: normal;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 .s7 {
                                    color: #0D0D0D;
                                    font-family: Arial, sans-serif;
                                    font-style:
                                        normal;
                                    font-weight: normal;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 .s8 {
                                    color: black;
                                    font-family: Verdana, sans-serif;
                                    font-style: normal;
                                    font-weight: normal;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 li {
                                    display: block;
                                }

                                 #l1 {
                                    padding-left: 0pt;
                                }

                                 #l1>li>*:first-child:before {
                                    content: ' ';
                                    color: black;
                                    font-family: Symbol, serif;
                                    font-style: normal;
                                    font-weight: normal;
                                    text-decoration: none;
                                    font-size: 8pt;
                                }

                                 table,
                                tbody {
                                    vertical-align: top;
                                    overflow: visible;
                                }

        
                            </style>
                        </head>

                        <body>
                            <p style='padding-top: 1pt;text-indent: 0pt;text-align: left;'><br /></p>
                            <table style='border-collapse:collapse;margin-left:7.9pt' cellspacing='0'>
                                <tr style='height:50pt'>
                                    <td style='width:263pt'>
                                        <p class='s1' style='padding-left: 2pt;text-indent:
                                            0pt;line-height: 12pt;text-align: left;'>{NombreNNA}</p>
                                        <p class='s2' style='padding-left: 2pt;text-indent: 0pt;line-height: 12pt;text-align: left;'>Edad:
                                            {edadNNA}</p>
                                        <p class='s2' style='padding-left: 2pt;text-indent: 0pt;line-height: 12pt;text-align:
                                            left;'>Diagnóstico:{DiagnosticoNNA}</p>
                                        <p class='s2' style='padding-left:
                                            2pt;text-indent: 0pt;line-height: 11pt;text-align: left;'>Fecha inicio seguimiento:
                                            {fechaInicioSeguimiento}</p>
                                    </td>
                                    <td style='width:227pt'>
                                        <p class='s2' style='padding-left: 68pt;padding-right: 2pt;text-indent: -7pt;text-align: right;'>
                                            Fecha generación: {FechaHoy}<br /> <u><b>{seguimientosRealizados} seguimientos
                                                    realizados</b><br /></u><b>
                                            </b><u><b>{SeguimientoId} Seguimiento en
                                                    proceso</b></u></p>
                                    </td>
                                </tr>
                            </table>
                            <p style='text-indent: 0pt;text-align: left;'><br /></p>
                            <p style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Datos básicos</p>
                            <p style='text-indent:
                                0pt;text-align: left;'><br /></p>
                            <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                <tr style='height:18pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Fecha de
                                            notificación del SIVIGILA</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>{fechaSivigila}</p>
                                    </td>
                                </tr>
                                <tr style='height:18pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Sexo</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>{sexo}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Tipo de identificación</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {tipoIdentificacion}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Número de identificación</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {numeroIdentificacion}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Fecha de nacimiento</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {fechaNacimiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>País de nacimiento</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {paisNacimiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Etnia</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>{etnia}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Departamento de nacimiento</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {departamentoNacimiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Ciudad de nacimiento</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {ciudadNacimiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Origen del reporte</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {origenReporte}</p>
                                    </td>
                                </tr>
                                <tr style='height:20pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align:
                                            left;'>Departamento donde actualmente recibe el tratamiento</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {departamentoTratamiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Estado de ingreso a la estrategia</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {departamentoTratamiento}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Fecha de ingreso a la estrategia</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {fechaIngresoEstrategia}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Grupo poblacional</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {grupoPoblacional}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Semanas de Gestación</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {semanasGestacion}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Régimen de afiliación</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>
                                            {regimenAfiliacion}</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Asegurador</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>{asegurador}
                                        </p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>IPS</p>
                                    </td>
                                    <td
                                        style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'>{Ips}</p>
                                    </td>
                                </tr>
                            </table>
                            <p style='padding-top: 9pt;text-indent:
                                0pt;text-align: left;'><br /></p>
                            <p style='padding-left: 40pt;text-indent:
                                0pt;text-align: left;'>Contactos</p>
                            <p style='text-indent: 0pt;text-align: left;'>
                                <br /></p>
                            <table style='border-collapse:collapse;margin-left:40.434pt' cellspacing='0'>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:106pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s5' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Nombre</p>
                                    </td>
                                    <td
                                        style='width:100pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s5' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Parentesco</p>
                                    </td>
                                    <td
                                        style='width:99pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s5' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Correo electrónico</p>
                                    </td>
                                    <td
                                        style='width:156pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s5' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Teléfono</p>
                                    </td>
                                </tr>
                                {contactos}        
                            </table>
                            <p style='padding-top: 9pt;text-indent:
                                0pt;text-align: left;'><br /></p>
                            <p style='padding-left: 5pt;text-indent:
                                0pt;text-align: left;'>Información del tratamiento</p>
                            <ul id='l1'>
                                <li data-list-text=''>
                                    <p style='padding-top: 8pt;padding-left: 40pt;text-indent: -17pt;text-align: left;'>
                                        Diagnóstico y tratamiento</p>
                                    <p style='text-indent: 0pt;text-align: left;'>
                                        <br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;padding-right: 5pt;text-indent:
                                                    0pt;line-height: 10pt;text-align: left;'>Razones por las cuales el menor no tiene
                                                    diagnóstico.</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {razonesNoTratamiento}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:11pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Diagnóstico</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {DiagnosticoNNA}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:11pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Fecha de consulta</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {fechaConsulta}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:11pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Fecha de diagnóstico</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {fechaDiagnostico}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:11pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Fecha de inicio de tratamiento</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {fechaInicioTratamiento}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:22pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align:
                                                    left;'>Nombre de la Institución en la que recibe</p>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;'>
                                                    tratamiento
                                                    actualmente (IPS)</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {IpsTratamiento}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 9pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 40pt;text-indent: -17pt;text-align: left;'>
                                        Recaídas</p>
                                    <p style='text-indent: 0pt;text-align: left;'><br />
                                    </p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Ha tenido recaídas</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {tieneRecaidas}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Número de recaidas</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {numeroRecaidas}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Fecha última recaída</p>
                                            </td>
                                            <td
                                                style='width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {fechaUltimaRecaida}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 8pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 40pt;text-indent: -17pt;text-align: left;'>
                                        Residencia y traslados</p>
                                    <p style='padding-top: 3pt;text-indent: 0pt;text-align: left;'>
                                        <br /></p>
                                    <h1 style='padding-left: 5pt;text-indent:
                                        0pt;text-align: left;'>Residencia de procedencia/ocurrencia</h1>
                                    <p style='text-indent:
                                        0pt;text-align: left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Departamento</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {departamentoResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Municipio</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {municipioResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Barrio</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {barrioResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Área</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {areaResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Dirección</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {direccionResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Estrato</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {estratoResidencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Teléfono</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {telefonoResidencia}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 9pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                    <p style='padding-left:
                                        5pt;text-indent: 0pt;text-align: left;'>Traslado</p>
                                    <p style='text-indent: 0pt;text-align:
                                        left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:30pt'>
                                            <td
                                                style='width:220pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-top: 5pt;padding-left: 5pt;padding-right:
                                                    27pt;text-indent: 0pt;text-align: left;'>Requirió trasladarse de ciudad para acceder al
                                                    tratamiento</p>
                                            </td>
                                            <td
                                                style='width:276pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {requirioTrasladarse}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 8pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                    <h1 style='padding-left:
                                        5pt;text-indent: 0pt;text-align: left;'>Residencia actual</h1>
                                    <p style='text-indent:
                                        0pt;text-align: left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Departamento</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {departamentoResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Municipio</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {municipioResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Barrio</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {barrioResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Área</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {areaResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Dirección</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {direccionResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Estrato</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {estratoResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Teléfono</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {telefonoResidenciaActual}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 6pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    10pt;text-align: left;'>Cuenta con la capacidad económica para asumir el traslado del menor
                                                </p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {capacidadEconomicaTraslado}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;padding-right: 9pt;text-indent:
                                                    0pt;line-height: 10pt;text-align: left;'>La EAPB le ha suministrado servicios sociales de
                                                    apoyo para el traslado</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {apoyoTraslado}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    10pt;text-align: left;'>¿Los servicios sociales de apoyo fueron entregados con oportunidad?
                                                </p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {apoyoOportuno}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    10pt;text-align: left;'>¿Los servicios sociales de apoyo logran dar cobertura al traslado
                                                    del menor?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {coberturaServicioSocial}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Nombre de la fundación</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {nombreFundacion}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Apoyo recibido por la fundación</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {apoyoFundacion}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>El sitio de residencia actual es</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {tipoResidenciaActual}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Quién asumió los costos del traslado?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {asumioCostosTraslado}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Quién asumió los costos de la vivienda?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {asumioCostosVivienda}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 9pt;text-indent:
                                        0pt;text-align: left;'><br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 40pt;text-indent: -17pt;text-align: left;'>
                                        Dificultades y traslados hospitalarios</p>
                                    <h1 style='padding-top: 8pt;padding-left:
                                        5pt;text-indent: 0pt;text-align: left;'>¿Ha presentado dificultades en los siguientes procesos?</h1>
                                    <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Autorización de medicamentos</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {dificultadAutorizacionMedicamentos}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Entrega de medicamentos LAP</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {dificultadEntregaMedicamentosLAP}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Entrega de medicamentos No LAP</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {dificultadEntregaMedicamentosNoLAP}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Asignación de citas</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {dificultadAsignacionCitas}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Le han cobrado copagos o cuotas moderadoras</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {HanCobradoCopago}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Autorización de procedimientos</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {AutorizacionProcedimiento}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Remisión a instituciones especializadas</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {remisionEspecialista}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Mala atención en la IPS</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>{MalaAtencionIps} {cualIps}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Falla en MIPRES</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {FallaMipres}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Falla en convenio entre la EAPB e IPS tratante</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {fallaConvenio}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='text-indent: 0pt;text-align: left;'>
                                        <br /></p>
                                    <p style='padding-left: 5pt;text-indent:
                                        0pt;text-align: left;'>Traslado de institución</p>
                                    <p style='text-indent: 0pt;text-align:
                                        left;'><br /></p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Ha sido trasladado de institución?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {HaTrasladado}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Número de traslados</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {numeroTraslados}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>IPS</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {ips}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:20pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    10pt;text-align: left;'>¿Ha tenido que recurrir a algún tipo de acción legal para acceder a
                                                    la atención?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {haRecurridoAccionLegal}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Motivo</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {Motivo}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Tipo de recurso</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {tipoRecurso}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 9pt;text-indent: 0pt;text-align: left;'>
                                        <br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 40pt;text-indent: -17pt;text-align: left;'>Adherencia al tratamiento y calendario
                                        escolar</p>
                                    <p style='text-indent: 0pt;text-align: left;'><br />
                                    </p>
                                    <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Ha dejado de asistir al tratamiento?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {haDejadoTratamiento}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Por cuánto tiempo ha dejado de asistir?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>{tiempoInasistenciaTratamiento}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Causas de inasistencia</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {causaInasistencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>Otra ¿Cuál?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {CualOtraCausaInasistencia}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s7' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Está estudiando actualmente?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {estudiaActualmente}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s7' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿En algún momento ha dejado de asistir al colegio?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {haDejadoColegio}</p>
                                            </td>
                                        </tr>
                                        <tr style='height:10pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s7' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Por cuánto tiempo ha dejado de asistir al colegio?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s6' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>{tiempoInasistenciaColegio</p>
                                            </td>
                                        </tr>
                                        <tr style='height:19pt'>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p class='s7' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    9pt;text-align: left;'>¿Considera que la IPS y/o el médico le han informado de manera</p>
                                                <p class='s7' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                                    8pt;text-align: left;'>clara y completa sobre el diagnóstico y el tratamiento del NNA?</p>
                                            </td>
                                            <td
                                                style='width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                                <p style='text-indent: 0pt;text-align: left;'>
                                                    {ipsClara}</p>
                                            </td>
                                        </tr>
                                    </table>
                                    <p style='padding-top: 9pt;text-indent: 0pt;text-align: left;'>
                                        <br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 40pt;text-indent: -17pt;text-align: left;'>Observaciones</p>
                                    <p class='s8' style='padding-top: 8pt;padding-left: 5pt;text-indent: 0pt;text-align: left;'>
                                        Observaciones diligenciadas en el último seguimiento.</p>
                                    <p style='text-indent:
                                        0pt;text-align: left;'><br /></p>
                                </li>
                                <li data-list-text=''>
                                    <p style='padding-left: 5pt;text-indent: 18pt;line-height:
                                        189%;text-align: left;'>Trazabilidad Seguimientos</p>
                                </li>
                            </ul>
                            <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>No.</p>
                                    </td>
                                    <td
                                        style='width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Fecha</p>
                                    </td>
                                    <td
                                        style='width:117pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Asunto</p>
                                    </td>
                                    <td
                                        style='width:269pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align:
                                            left;'>Observación</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:117pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:269pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                </tr>
                            </table>
                            <p style='padding-top: 8pt;text-indent:
                                0pt;text-align: left;'><br /></p>
                            <p style='padding-left: 5pt;text-indent:
                                0pt;text-align: left;'>Alertas</p>
                            <p style='text-indent: 0pt;text-align: left;'>
                                <br /></p>
                            <table style='border-collapse:collapse;margin-left:5.25pt' cellspacing='0'>
                                <tr style='height:39pt'>
                                    <td
                                        style='width:60pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;padding-right: 4pt;text-indent: 0pt;text-align:
                                            left;'>No. seguimiento</p>
                                    </td>
                                    <td
                                        style='width:56pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;padding-right: 4pt;text-indent: 0pt;text-align:
                                            left;'>Fecha notificación</p>
                                    </td>
                                    <td
                                        style='width:50pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Categoría
                                        </p>
                                    </td>
                                    <td
                                        style='width:63pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>
                                            Subcategoría</p>
                                    </td>
                                    <td
                                        style='width:69pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Entidad(es)
                                            sobre la(s)</p>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;line-height:
                                            10pt;text-align: left;'>que se genera alerta</p>
                                    </td>
                                    <td
                                        style='width:134pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Observación
                                        </p>
                                    </td>
                                    <td
                                        style='width:64pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p class='s4' style='padding-left: 5pt;text-indent: 0pt;text-align: left;'>Estado</p>
                                    </td>
                                </tr>
                                <tr style='height:10pt'>
                                    <td
                                        style='width:60pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:56pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:50pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:63pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:69pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:134pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                    <td
                                        style='width:64pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                        <p style='text-indent: 0pt;text-align: left;'><br /></p>
                                    </td>
                                </tr>
                            </table>
                        </body>
                    </html>";

                    var contactos = @"
                        <tr style='height:10pt'>
                            <td
                                style='width:106pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                <p style='text-indent: 0pt;text-align: left;'>{NombreContacto}</p>
                            </td>
                            <td
                                style='width:100pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                <p style='text-indent: 0pt;text-align: left;'>{ParentescoContacto}</p>
                            </td>
                            <td
                                style='width:99pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                <p style='text-indent: 0pt;text-align: left;'>{CorreoContacto}</p>
                            </td>
                            <td
                                style='width:156pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt'>
                                <p style='text-indent: 0pt;text-align: left;'>{TelefonoContacto}</p>
                            </td>
                        </tr>
                    ";
                    #endregion

                    htmlContent = htmlContent.Replace("{NombreNNA}", seguimiento.Nombre)
                        .Replace("{FechaHoy}", DateTime.Now.ToString("dd/MM/yyyy"))
                        .Replace("{edadNNA}", edad.ToString())
                        .Replace("{DiagnosticoNNA}", seguimiento.Diagnostico)
                        .Replace("{fechaInicioSeguimiento}", seguimiento.FechaSeguimiento == null ? "" : seguimiento.FechaSeguimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaSivigila}", seguimiento.FechaSeguimiento == null ? "" : seguimiento.FechaSeguimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{sexo}", seguimiento.IdSexo == "M" ? "Masculino" : "Femenino")
                        .Replace("{tipoIdentificacion}", tipoIdentificacion.FirstOrDefault(x => x.Codigo == seguimiento.TipoIdentificacion)?.Nombre ?? "")
                        .Replace("{numeroIdentificacion}", seguimiento.NumeroIdentificacion)
                        .Replace("{seguimientosRealizados}", seguimiento.CantidadSegumientos.ToString())
                        .Replace("{SeguimientoId}", seguimiento.Id.ToString())
                        .Replace("{fechaNacimiento}", seguimiento.FechaNacimiento == null ? "" : seguimiento.FechaNacimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{paisNacimiento}", paises.FirstOrDefault(x => x.Codigo == seguimiento.PaisNacimiento)?.Nombre ?? "")
                        .Replace("{etnia}", GruposEtnicos.FirstOrDefault(x => x.Codigo == seguimiento.Etnia)?.Nombre ?? "")
                        .Replace("{departamentoNacimiento}", departamentos.FirstOrDefault(x => x.Codigo == seguimiento.DepartamentoNacimiento)?.Nombre ?? "")
                        .Replace("{ciudadNacimiento}", municipios.FirstOrDefault(x => x.Codigo == seguimiento.CiudadNacimiento)?.Nombre ?? "")
                        .Replace("{origenReporte}", origenReporte != null ? origenReporte.Nombre : "")
                        .Replace("{departamentoTratamiento}", departamentos.FirstOrDefault(x => x.Codigo == seguimiento.DepartamentoTratamiento)?.Nombre ?? "")
                        .Replace("{fechaIngresoEstrategia}", seguimiento.FechaIngreso == null ? "" : seguimiento.FechaIngreso.Value.ToString("dd/MM/yyyy"))
                        .Replace("{grupoPoblacional}", tiposPoblacion.FirstOrDefault(x => x.Codigo == seguimiento.GrupoPoblacional)?.Nombre ?? "")
                        .Replace("{semanasGestacion}", seguimiento.SemanasGestacion.ToString())
                        .Replace("{regimenAfiliacion}", regimenes.FirstOrDefault(x => x.Codigo == seguimiento.RegimenAfiliacion)?.Nombre ?? "")
                        .Replace("{asegurador}", seguimiento.Asegurador.ToString())
                        .Replace("{Ips}", ips != null ? ips.Nombre : "")
                        .Replace("{razonesNoTratamiento}", seguimiento.razonesNoTratamiento)
                        .Replace("{fechaConsulta}", seguimiento.fechaConsulta == null ? "" : seguimiento.fechaConsulta.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaDiagnostico}", seguimiento.fechaDiagnostico == null ? "" : seguimiento.fechaDiagnostico.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaInicioTratamiento}", seguimiento.fechaInicioTratamiento == null ? "" : seguimiento.fechaInicioTratamiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{IpsTratamiento}", seguimiento.IpsTratamiento)
                        .Replace("{tieneRecaidas}", seguimiento.tieneRecaidas == null ? "NA" : seguimiento.tieneRecaidas.Value ? "SI" : "NO")
                        .Replace("{numeroRecaidas}", seguimiento.numeroRecaidas == null ? "0" : seguimiento.numeroRecaidas.ToString())
                        .Replace("{fechaUltimaRecaida}", seguimiento.fechaUltimaRecaida == null ? "" : seguimiento.fechaUltimaRecaida.Value.ToString("dd/MM/yyyy"))
                        .Replace("{departamentoResidencia}", departamentos.FirstOrDefault(x => x.Codigo == seguimiento.departamentoResidencia)?.Nombre ?? "")
                        .Replace("{municipioResidencia}", municipios.FirstOrDefault(x => x.Codigo == seguimiento.municipioResidencia)?.Nombre ?? "")
                        .Replace("{barrioResidencia}", seguimiento.barrioResidencia)
                        .Replace("{areaResidencia}", zonas.FirstOrDefault(x => x.Codigo == seguimiento.areaResidencia)?.Nombre ?? "")
                        .Replace("{direccionResidencia}", seguimiento.direccionResidencia)
                        .Replace("{estratoResidencia}", estratos.FirstOrDefault(x => x.Codigo == seguimiento.estratoResidencia)?.Nombre ?? "")
                        .Replace("{telefonoResidencia}", seguimiento.telefonoResidencia)
                        .Replace("{requirioTrasladarse}", seguimiento.requirioTrasladarse ? "SI" : "NO")
                        .Replace("{departamentoResidenciaActual}", departamentos.FirstOrDefault(x => x.Codigo == seguimiento.departamentoResidenciaActual)?.Nombre ?? "")
                        .Replace("{municipioResidenciaActual}", municipios.FirstOrDefault(x => x.Codigo == seguimiento.municipioResidenciaActual)?.Nombre ?? "")
                        .Replace("{barrioResidenciaActual}", seguimiento.barrioResidenciaActual)
                        .Replace("{areaResidenciaActual}", zonas.FirstOrDefault(x => x.Codigo == seguimiento.areaResidenciaActual)?.Nombre ?? "")
                        .Replace("{direccionResidenciaActual}", seguimiento.direccionResidenciaActual)
                        .Replace("{estratoResidenciaActual}", estratos.FirstOrDefault(x => x.Codigo == seguimiento.estratoResidenciaActual)?.Nombre ?? "")
                        .Replace("{telefonoResidenciaActual}", seguimiento.telefonoResidencia)
                        .Replace("{capacidadEconomicaTraslado}", seguimiento.capacidadEconomicaTraslado == null ? "NA" : seguimiento.capacidadEconomicaTraslado.Value ? "SI" : "NO")
                        .Replace("{apoyoTraslado}", seguimiento.apoyoTraslado == null ? "NA" : seguimiento.apoyoTraslado.Value ? "SI" : "NO")
                        .Replace("{apoyoOportuno}", seguimiento.apoyoOportuno == null ? "NA" : seguimiento.apoyoOportuno.Value ? "SI" : "NO")
                        .Replace("{coberturaServicioSocial}", seguimiento.coberturaServicioSocial == null ? "NA" : seguimiento.coberturaServicioSocial.Value ? "SI" : "NO")
                        .Replace("{nombreFundacion}", seguimiento.nombreFundacion)
                        .Replace("{apoyoFundacion}", seguimiento.apoyoFundacion)
                        .Replace("{tipoResidenciaActual}", tiposVivienda.FirstOrDefault(x => x.Codigo == seguimiento.tipoResidenciaActual)?.Nombre ?? "")
                        .Replace("{asumioCostosTraslado}", seguimiento.asumioCostosTraslado)
                        .Replace("{asumioCostosVivienda}", seguimiento.asumioCostosVivienda)
                        .Replace("{dificultadAutorizacionMedicamentos}", seguimiento.dificultadAutorizacionMedicamentos == null ? "NA" : seguimiento.dificultadAutorizacionMedicamentos.Value ? "SI" : "NO")
                        .Replace("{dificultadEntregaMedicamentosLAP}", seguimiento.dificultadEntregaMedicamentosLAP == null ? "NA" : seguimiento.dificultadEntregaMedicamentosLAP.Value ? "SI" : "NO")
                        .Replace("{dificultadEntregaMedicamentosNoLAP}", seguimiento.dificultadEntregaMedicamentosNoLAP == null ? "NA" : seguimiento.dificultadEntregaMedicamentosNoLAP.Value ? "SI" : "NO")
                        .Replace("{dificultadAsignacionCitas}", seguimiento.dificultadAsignacionCitas == null ? "NA" : seguimiento.dificultadAsignacionCitas.Value ? "SI" : "NO")
                        .Replace("{HanCobradoCopago}", seguimiento.HanCobradoCopago == null ? "NA" : seguimiento.HanCobradoCopago.Value ? "SI" : "NO")
                        .Replace("{AutorizacionProcedimiento}", seguimiento.AutorizacionProcedimiento == null ? "NA" : seguimiento.AutorizacionProcedimiento.Value ? "SI" : "NO")
                        .Replace("{remisionEspecialista}", seguimiento.remisionEspecialista == null ? "NA" : seguimiento.remisionEspecialista.Value ? "SI" : "NO")
                        .Replace("{MalaAtencionIps} {cualIps}", seguimiento.MalaAtencionIps == null ? "NA" : seguimiento.MalaAtencionIps.Value ? ("SI" + seguimiento.cualIps.ToString() == null ? "" : seguimiento.cualIps.ToString()) : "NO")
                        .Replace("{FallaMipres}", seguimiento.FallaMipres == null ? "NA" : seguimiento.FallaMipres.Value ? "SI" : "NO")
                        .Replace("{fallaConvenio}", seguimiento.fallaConvenio == null ? "NA" : seguimiento.fallaConvenio.Value ? "SI" : "NO")
                        .Replace("{HaTrasladado}", seguimiento.HaTrasladado == null ? "NA" : seguimiento.HaTrasladado.Value ? "SI" : "NO")
                        .Replace("{ips}", seguimiento.ips == null ? "NA" : seguimiento.ips.ToString())
                        .Replace("{haRecurridoAccionLegal}", seguimiento.haRecurridoAccionLegal == null ? "NA" : seguimiento.haRecurridoAccionLegal.Value ? "SI" : "NO")
                        .Replace("{Motivo}", seguimiento.Motivo)
                        .Replace("{tipoRecurso}", seguimiento.tipoRecurso)
                        .Replace("{haDejadoTratamiento}", seguimiento.haDejadoTratamiento == null ? "NA" : seguimiento.haDejadoTratamiento.Value ? "SI" : "NO")
                        .Replace("{tiempoInasistenciaTratamiento}", seguimiento.tiempoInasistenciaTratamiento == null ? "" : seguimiento.tiempoInasistenciaTratamiento.ToString())
                        .Replace("{causaInasistencia}", seguimiento.causaInasistencia)
                        .Replace("{CualOtraCausaInasistencia}", seguimiento.CualOtraCausaInasistencia)
                        .Replace("{estudiaActualmente}", seguimiento.estudiaActualmente == null ? "NA" : seguimiento.estudiaActualmente.Value ? "SI" : "NO")
                        .Replace("{haDejadoColegio}", seguimiento.haDejadoColegio == null ? "NA" : seguimiento.haDejadoColegio.Value ? "SI" : "NO")
                        .Replace("{tiempoInasistenciaColegio", seguimiento.tiempoInasistenciaColegio == null ? "" : seguimiento.tiempoInasistenciaColegio.ToString())
                        .Replace("{ipsClara}", seguimiento.ipsClara == null ? "NA" : seguimiento.ipsClara.Value ? "SI" : "NO");

                    var contactosHtml = string.Empty;
                    seguimiento.Contactos.ForEach(x =>
                    {
                        contactosHtml += contactos.Replace("{NombreContacto}", x.Nombre)
                            .Replace("{ParentescoContacto}", x.Parentesco)
                            .Replace("{CorreoContacto}", x.CorreoElectronico)
                            .Replace("{TelefonoContacto}", x.Telefono);
                    });

                    htmlContent = htmlContent.Replace("{contactos}", contactosHtml);

                    using (var pdfStream = new MemoryStream())
                    {
                        // Convertir HTML a PDF usando el MemoryStream
                        HtmlConverter.ConvertToPdf(htmlContent, pdfStream);

                        // Convertir el MemoryStream a un array de bytes
                        byte[] pdfBytes = pdfStream.ToArray();

                        // Convertir el PDF a Base64
                        var base64Pdf = Convert.ToBase64String(pdfBytes);

                        File.WriteAllBytes("C:\\Users\\Giroco\\Documents\\DetalleSeguimiento.pdf", pdfBytes);

                        // Retornar el PDF en Base64
                        response.Base64 = base64Pdf;
                    }
                }
            }
            catch (PdfException)
            {
                response.Nombre = "Ha ocurrido un error";
            }
            catch (Exception)
            {
                response.Nombre = "Ha ocurrido un error";
            }

            return response;
        }

        public async Task<SeguimientoDto[]> GetSeguimientosEstados(string id)
        {
            var query = from s in _context.Seguimientos
                        join n in _context.NNAs on s.NNAId equals n.Id
                        where s.UsuarioId == id
                        group s by s.NNAId into g
                        select new { id = g.Max(x => x.Id) };

            return await (from q in query
                          join s in _context.Seguimientos on q.id equals s.Id
                          join n in _context.NNAs on s.NNAId equals n.Id

                          join p in _context.TPParentescos on n.CuidadorParentescoId equals p.Id into parentesco
                          from p in parentesco.DefaultIfEmpty()

                          join d in _context.CIE10s on n.DiagnosticoId equals d.Id into diagnostico
                          from d in diagnostico.DefaultIfEmpty()

                          join a in _context.UsuarioAsignados on s.Id equals a.SeguimientoId into asignado
                          from a in asignado.DefaultIfEmpty()

                          join ea in _context.TPEAPB on n.EAPBId equals ea.Id into eapb
                          from ea in eapb.DefaultIfEmpty()

                          join e in _context.TPEstadoNNA on n.estadoId equals e.Id
                          select new SeguimientoDto()
                          {
                              Id = s.Id,
                              NoCaso = s.NNAId,
                              PrimerNombre = n.PrimerNombre,
                              SegundoNombre = n.SegundoNombre,
                              PrimerApellido = n.PrimerApellido,
                              SegundoApellido = n.SegundoApellido,
                              Sexo = n.SexoId == "1" ? "Masculino" : "Femenino",
                              FechaNacimiento = n.FechaNacimiento,
                              FechaNotificacion = n.FechaNotificacionSIVIGILA,

                              FechaSolicitud = s.FechaSolicitud, // solicitado
                              FechaAsignacion = a != null ? a.FechaAsignacion : null, // fecha asignacion
                              FechaSeguimiento = s.FechaSeguimiento, // agendado
                              FechaUltimaActuacion = s.UltimaActuacionFecha, // contacto

                              EstadoSeguimiento = (s.UltimaActuacionFecha != null ? "Contactado" : (s.FechaSeguimiento != null ? "Agendado" : (a.FechaAsignacion != null ? "Asignado" : (s.FechaSolicitud != null ? "Solicitado" : "")))),

                              TipoIdentificacion = n.TipoIdentificacionId,
                              NumeroIdentificacion = n.NumeroIdentificacion,
                              Parentesco = p != null ? p.Nombre : "",
                              Diagnostico = d != null ? d.Nombre : "",
                              Aseguradora = ea != null ? ea.Nombre : "",
                              AsuntoUltimaActuacion = s.UltimaActuacionAsunto,
                          }).ToArrayAsync();
        }
    }
}
