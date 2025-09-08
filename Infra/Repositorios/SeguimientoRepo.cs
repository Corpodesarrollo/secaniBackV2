using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Core.Utilities;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Exceptions;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
                        join ua in _context.UsuarioAsignados on s.Id equals ua.SeguimientoId
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
                                        FechaIngresoEstrategia = n.FechaIngresoEstrategia
                                    }).FirstOrDefaultAsync();

                if (result != null)
                {
                    var seguimiento = await (from s in _context.Seguimientos
                                             join e in _context.TPEstadoSeguimiento on s.EstadoId equals e.Id
                                             where s.NNAId == id
                                             orderby s.FechaSeguimiento descending
                                             select new
                                             {
                                                 s.FechaSeguimiento,
                                                 s.FechaSolicitud,
                                                 s.EstadoId,
                                                 Estado = e.Nombre
                                             }).FirstOrDefaultAsync();

                    result.FechaInicioSeguimiento = seguimiento?.FechaSeguimiento;
                    result.Estado = seguimiento?.Estado;
                    result.SeguimientosRealizados = await _context.Seguimientos.CountAsync(s => s.NNAId == id);
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
                               IdAlertaSeguimiento = alert.Id,
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

        public async Task<long> SetSeguimiento(SetSeguimientoRequest request)
        {
            try
            {
                var ultimoSeguimiento = await _context.Seguimientos.Where(s => s.NNAId == request.NNAId).OrderByDescending(x => x.FechaSeguimiento).FirstOrDefaultAsync();
                if (ultimoSeguimiento != null)
                {
                    ultimoSeguimiento.Telefono = request.Telefono;
                    ultimoSeguimiento.ObservacionAgente = request.ObservacionAgente;
                    ultimoSeguimiento.ObservacionesSolicitante = request.ObservacionesSolicitante;
                    ultimoSeguimiento.UltimaActuacionFecha = DateTime.Now;
                    ultimoSeguimiento.UltimaActuacionAsunto = request.UltimaActuacionAsunto;
                    ultimoSeguimiento.FechaSeguimiento = DateTime.Now;
                    ultimoSeguimiento.UpdatedByUserId = request.UsuarioId;
                    ultimoSeguimiento.DateUpdated = DateTime.Now;

                    _context.Seguimientos.Update(ultimoSeguimiento);
                }

                var seguimiento = new Seguimiento()
                {
                    NNAId = request.NNAId,
                    FechaSeguimiento = request.FechaSeguimiento,
                    EstadoId = request.EstadoId,
                    ContactoNNAId = request.ContactoNNAId,
                    UsuarioId = request.UsuarioId,
                    SolicitanteId = request.SolicitanteId,
                    FechaSolicitud = request.FechaSolicitud,
                    TieneDiagnosticos = request.TieneDiagnosticos,
                    UltimaActuacionFecha = request.UltimaActuacionFecha,
                    NombreRechazo = request.NombreRechazo,
                    ParentescoRechazo = request.ParentescoRechazo,
                    RazonesRechazo = request.RazonesRechazo,
                    CreatedByUserId = "1"
                };
                _context.Seguimientos.Add(seguimiento);
                await _context.SaveChangesAsync();

                var usuarioAsignado = new UsuarioAsignado()
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = request.FechaSeguimiento,
                    Observaciones = "Asignación automática",
                    SeguimientoId = seguimiento.Id,
                    UsuarioId = request.UsuarioId,
                };
                _context.UsuarioAsignados.Add(usuarioAsignado);
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
                            UltimaFechaSeguimiento = ultimoSeguimiento.FechaSeguimiento
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
                            if ((item.Resuelta ?? false) == false)
                            {
                                var alertaSeguimiento2 = new AlertaSeguimiento()
                                {
                                    AlertaId = item.Id ?? 0,
                                    CreatedByUserId = "1",
                                    DateCreated = DateTime.Now,
                                    EstadoId = item.Resuelta ?? false ? 4 : 3,
                                    SeguimientoId = seguimiento.Id,
                                    Observaciones = item.Resuelta ?? false ? "Alerta resuelta en seguimiento" : "Alerta sin resolver en seguimiento",
                                    UltimaFechaSeguimiento = DateTime.Now
                                };
                                _context.AlertaSeguimientos.Add(alertaSeguimiento2);
                                await _context.SaveChangesAsync();
                            }

                            alertaSeguimiento.EstadoId = item.Resuelta ?? false ? 4 : 3;
                            alertaSeguimiento.Observaciones = item.Resuelta ?? false ? "Alerta resuelta en seguimiento" : "Alerta sin resolver en seguimiento";
                            alertaSeguimiento.UltimaFechaSeguimiento = DateTime.Now;
                            _context.AlertaSeguimientos.Update(alertaSeguimiento);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return seguimiento.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<UsuarioAsignado>> AsignacionAutomatica()
        {
            var seguimientosAsignados = new List<UsuarioAsignado>();

            var seguimientosNoAsignados = await CargarSeguimientos();

            var fecha = DateTime.Now.Date;
            var revisores = await CargarRevisores(fecha);

            while (seguimientosNoAsignados.Count > 0)
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
                        revisor.CantidadSeguimientosDisponibles = revisor.CantidadSeguimientos - seguimientosAsignadosFecha.Count;

                    //valida si el revisor tiene seguimeintos disponibles por asignar
                    if (revisor.CantidadSeguimientosDisponibles <= 0)
                    {
                        revisores.Remove(revisor);
                        continue;
                    }

                    //validar lista de seguimientos no asignados
                    if (seguimientosNoAsignados.Count == 0)
                        break;

                    //el revisor entra a las horaentrada y sale a la horasalida. se debe asignar el seguimiento en un rango de 640 segundos,
                    //si el seguimiento se cruza con otro se debe aumentar 640 segundos  y volver a verificar hasta lograr agendar el seguimiento
                    var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);
                    if (fechaAsignacion == null)
                        continue;

                    var seguimiento = seguimientosNoAsignados[0];

                    //asignar seguimiento al revisor
                    var usuarioAsignado = new UsuarioAsignado
                    {
                        Activo = true,
                        DateCreated = DateTime.Now,
                        FechaAsignacion = fechaAsignacion,
                        Observaciones = "Asignación automática",
                        SeguimientoId = seguimiento.Item1,
                        NombreNNA = seguimiento.Item2,
                        DocumentoNNA = seguimiento.Item3,
                        UsuarioId = revisor.UserId,
                        NombreUsuario = revisor.Nombre,
                    };

                    _context.UsuarioAsignados.Add(usuarioAsignado);
                    await _context.SaveChangesAsync();

                    //actualizar fecha seguimiento
                    await ActulizarSeguimiento(fechaAsignacion, seguimiento.Item1, revisor.UserId, "Registro Inicial");

                    seguimientosAsignados.Add(usuarioAsignado);
                    seguimientosNoAsignados.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                }

                fecha = fecha.AddDays(1);
                revisores = await CargarRevisores(fecha);

                //validar si hay disponibilidad de revisores
                var revisoresDisponibles = await ValidarDiponibilidadAgentes(fecha);
                if (!revisoresDisponibles)
                    break;
            }

            return seguimientosAsignados;
        }

        private async Task ActulizarSeguimiento(DateTime? fechaAsignacion, long idSeguimiento, string idUsuario, string asunto = "")
        {
            var seguimientoActualizado = await _context.Seguimientos.FirstOrDefaultAsync(x => x.Id == idSeguimiento);
            if (seguimientoActualizado != null)
            {
                seguimientoActualizado.EstadoId = 1;
                seguimientoActualizado.UltimaActuacionFecha = fechaAsignacion;
                seguimientoActualizado.UltimaActuacionAsunto = asunto;
                seguimientoActualizado.FechaSeguimiento = fechaAsignacion;
                seguimientoActualizado.UsuarioId = idUsuario;
                _context.Seguimientos.Update(seguimientoActualizado);
                await _context.SaveChangesAsync();
            }
        }

        private static DateTime? BuscarEspacioHorario(DateTime fecha, UsuariosHorariosDto revisor, List<UsuarioAsignado> seguimientosAsignadosFecha)
        {
            var fechaAsignacion = fecha.Date + revisor.HoraEntrada.GetValueOrDefault();
            var fechaSalida = fecha.Date + revisor.HoraSalida.GetValueOrDefault();
            var fechaEncontrada = false;

            if (fechaAsignacion < DateTime.Now)
                fechaAsignacion = DateTime.Now;

            while (!fechaEncontrada && fechaAsignacion < fechaSalida)
            {
                var proxFechaAsignacion = fechaAsignacion.AddSeconds(640);
                var seguimientosAsignadosFechaRango = seguimientosAsignadosFecha.Where(x => x.FechaAsignacion >= fechaAsignacion && x.FechaAsignacion < proxFechaAsignacion).FirstOrDefault();
                if (seguimientosAsignadosFechaRango != null)
                {
                    fechaAsignacion = fechaAsignacion.AddSeconds(640);
                    continue;
                }
                else
                    fechaEncontrada = true;
            }

            if (!fechaEncontrada)
                return null;

            return fechaAsignacion;
        }

        public async Task<List<UsuarioAsignado>> AsignacionAutomaticaReagendar()
        {
            var fecha = DateTime.Now;
            var seguimientosReagendados = new List<UsuarioAsignado>();

            //reagendar por no ejecucion de seguimiento
            var revisores = await CargarRevisores(fecha);

            foreach (var revisor in revisores)
            {
                var seguimientos = from s in _context.Seguimientos
                                   join n in _context.NNAs on s.NNAId equals n.Id
                                   where s.UsuarioId == revisor.UserId
                                   group s by s.NNAId into g
                                   select new { id = g.Max(x => x.Id) };

                var seguimientosReagendamiento = await (from q in seguimientos
                                                        join seg in _context.Seguimientos on q.id equals seg.Id
                                                        join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                        join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId
                                                        where seg.FechaSeguimiento < fecha && ua.UsuarioId == revisor.UserId
                                                        select new ValueTuple<long, string, string>(
                                                            seg.Id,
                                                            $"{nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}",
                                                            nna.NumeroIdentificacion ?? ""
                                                        )).ToListAsync();

                fecha = await ReagendarSeguimientos(fecha, seguimientosReagendados, revisor, seguimientosReagendamiento, "No ejecución");
            }

            //reagendar por agente ausente
            var revisoresAusentes = await CargarRevisoresAusentes(fecha);

            foreach (var revisor in revisoresAusentes)
            {
                var seguimientosReagendamiento = await (from seg in _context.Seguimientos
                                                        join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                        join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId
                                                        where ua.FechaAsignacion == fecha && ua.UsuarioId == revisor.UserId
                                                        select new ValueTuple<long, string, string>(
                                                            seg.Id,
                                                            $"{nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}",
                                                            nna.NumeroIdentificacion ?? ""
                                                        )).ToListAsync();

                fecha = await ReagendarSeguimientos(fecha, seguimientosReagendados, revisor, seguimientosReagendamiento, "Ausencia");
            }

            return seguimientosReagendados;
        }

        private async Task<DateTime> ReagendarSeguimientos(DateTime fecha, List<UsuarioAsignado> seguimientosReagendados, UsuariosHorariosDto revisor, List<(long, string, string)> seguimientosReagendamiento, string tipo)
        {
            while (seguimientosReagendamiento.Count > 0)
            {
                fecha = fecha.Date.AddDays(1);

                //validar que la fecha no es dia festivo
                var festivos = await _context.TPFestivos.FirstOrDefaultAsync(x => x.Festivo == fecha);
                if (festivos != null)
                    continue;

                //validamos los seguimientos asignados al revisor en la fecha
                var fechaIni = fecha;
                var fechaFin = fechaIni.AddDays(1).AddSeconds(-1);
                var seguimientosAsignadosFecha = await _context.UsuarioAsignados.Where(x => x.UsuarioId == revisor.UserId && x.FechaAsignacion >= fechaIni && x.FechaAsignacion <= fechaFin).ToListAsync();
                if (seguimientosAsignadosFecha.Count > 0)
                    revisor.CantidadSeguimientosDisponibles = revisor.CantidadSeguimientos - seguimientosAsignadosFecha.Count;

                //valida si el revisor tiene seguimeintos disponibles por asignar
                if (revisor.CantidadSeguimientosDisponibles <= 0)
                    continue;

                var seguimiento = seguimientosReagendamiento[0];
                var continuar = true;
                do
                {
                    // validacion para verificar que el seguimiento no haya sido reagendado previamente
                    var seguimientosReasignados = await _context.UsuarioAsignados
                        .Where(x => x.SeguimientoId == seguimiento.Item1 && x.FechaAsignacion > fecha)
                        .AnyAsync();

                    if (seguimientosReasignados)
                    {
                        seguimientosReagendamiento.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                        if (seguimientosReagendamiento.Count > 0)
                            seguimiento = seguimientosReagendamiento[0];
                        else
                            continuar = false;
                    }
                    else
                        continuar = false;

                } while (continuar);

                if (seguimientosReagendamiento.Count == 0)
                    break;

                var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);
                if (fechaAsignacion == null)
                    continue;

                //asignar seguimiento al revisor
                var usuarioAsignado = new UsuarioAsignado
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = fechaAsignacion,
                    Observaciones = "Reagendamiento automático",
                    SeguimientoId = seguimiento.Item1,
                    NombreNNA = seguimiento.Item2,
                    DocumentoNNA = seguimiento.Item3,
                    Criterio = tipo,
                    UsuarioId = revisor.UserId
                };

                _context.UsuarioAsignados.Add(usuarioAsignado);
                await _context.SaveChangesAsync();

                //actualizar fecha seguimiento
                await ActulizarSeguimiento(fechaAsignacion, seguimiento.Item1, revisor.UserId);

                seguimientosReagendados.Add(usuarioAsignado);
                seguimientosReagendamiento.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados

                //validar si hay disponibilidad de revisores
                var revisoresDisponibles = await _context.HorarioLaboralAgente.AnyAsync(x => x.UserId == revisor.UserId && x.Fecha == fecha);
                if (!revisoresDisponibles)
                    break;
            }

            return fecha;
        }

        private async Task<List<UsuariosHorariosDto>> CargarRevisoresAusentes(DateTime fecha)
        {
            var diaSemana = (int)fecha.DayOfWeek;
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fecha } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && aus != null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).ToListAsync();
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
                                                  join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                  join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId
                                                  join u in _context.Users on ua.UsuarioId equals u.Id
                                                  where ua.FechaAsignacion == fecha && u.Activo == false
                                                  select new ValueTuple<long, string, string>(
                                                      seg.Id,
                                                      $"{nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}",
                                                      nna.NumeroIdentificacion
                                                  )).ToListAsync();

            var revisores = await CargarRevisoresReasignacion(fecha);

            while (seguimientosReasignacion.Count > 0)
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

                    var seguimiento = seguimientosReasignacion[0];
                    var continuar = true;
                    do
                    {
                        // validacion para verificar que el seguimiento no haya sido reagendado previamente
                        var seguimientosReasignados = await _context.UsuarioAsignados
                            .Where(x => x.SeguimientoId == seguimiento.Item1 && x.FechaAsignacion > fecha)
                            .AnyAsync();

                        if (seguimientosReasignados)
                        {
                            seguimientosReasignacion.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                            if (seguimientosReasignacion.Count > 0)
                                seguimiento = seguimientosReasignacion[0];
                            else
                                continuar = false;
                        }
                        else
                            continuar = false;

                    } while (continuar);

                    if (seguimientosReasignacion.Count == 0)
                        break;

                    var fechaAsignacion = BuscarEspacioHorario(fecha, revisor, seguimientosAsignadosFecha);
                    if (fechaAsignacion == null)
                        continue;

                    //asignar seguimiento al revisor
                    var usuarioAsignado = new UsuarioAsignado
                    {
                        Activo = true,
                        DateCreated = DateTime.Now,
                        FechaAsignacion = fechaAsignacion,
                        Observaciones = "Asignación automática",
                        SeguimientoId = seguimiento.Item1,
                        NombreNNA = seguimiento.Item2,
                        UsuarioId = revisor.UserId
                    };

                    _context.UsuarioAsignados.Add(usuarioAsignado);
                    await _context.SaveChangesAsync();

                    //actualizar fecha seguimiento
                    await ActulizarSeguimiento(fechaAsignacion, seguimiento.Item1, revisor.UserId);

                    seguimientosAsignados.Add(usuarioAsignado);

                    seguimientosReasignacion.Remove(seguimiento); //se quita el seguimiento de la lista de no asignados
                    revisor.CantidadSeguimientosDisponibles--;
                }

                fecha = fecha.AddDays(1);
                revisores = await CargarRevisoresReasignacion(fecha);

                //validar si hay disponibilidad de revisores
                var revisoresDisponibles = await ValidarDiponibilidadAgentes(fecha);
                if (!revisoresDisponibles)
                    break;
            }

            return seguimientosAsignados;
        }

        private async Task<List<UsuariosHorariosDto>> CargarRevisoresReasignacion(DateTime fecha)
        {
            var diaSemana = (int)fecha.DayOfWeek;

            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fecha } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Dia == diaSemana && aus == null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Nombre = u.FullName,
                              Email = u.Email,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).ToListAsync();
        }

        private async Task<List<(long, string, string)>> CargarSeguimientos()
        {
            return await (from seg in _context.Seguimientos
                          join nna in _context.NNAs on seg.NNAId equals nna.Id
                          join ua in _context.UsuarioAsignados on seg.Id equals ua.SeguimientoId into uaJoin
                          from uas in uaJoin.DefaultIfEmpty()
                          where nna.estadoId == 15 && uas == null
                          select new ValueTuple<long, string, string>(
                              seg.Id,
                              $"{nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}",
                              nna.NumeroIdentificacion ?? ""
                          )).ToListAsync();
        }

        private async Task<bool> ValidarDiponibilidadAgentes(DateTime fecha)
        {
            var fechaValidar = fecha;
            var diaSemana = (int)fechaValidar.DayOfWeek;

            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fechaValidar } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Dia == diaSemana && aus == null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Nombre = u.FullName,
                              Email = u.Email,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).AnyAsync();
        }

        private async Task<List<UsuariosHorariosDto>> CargarRevisores(DateTime fecha)
        {
            var fechaValidar = fecha;
            var diaSemana = (int)fechaValidar.DayOfWeek;

            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          join h in _context.HorarioLaboralAgente on u.Id equals h.UserId
                          join a in _context.Ausencias on new { a = u.Id, b = fechaValidar } equals new { a = a.UsuarioId, b = a.FechaAusencia } into a
                          from aus in a.DefaultIfEmpty()
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true && h.Dia == diaSemana && aus == null
                          select new UsuariosHorariosDto
                          {
                              UserId = u.Id,
                              Nombre = u.FullName,
                              Email = u.Email,
                              Fecha = h.Fecha,
                              HoraEntrada = h.HoraEntrada,
                              HoraSalida = h.HoraSalida
                          }).ToListAsync();
        }

        public async Task<UserDto[]> CargarRevisores()
        {
            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046" && u.Activo == true
                          select new UserDto
                          {
                              Id = u.Id,
                              Alias = u.Alias,
                              Email = u.Email,
                              Name = u.FullName
                          }).ToArrayAsync();
        }

        public async Task<UserDto[]> CargarCoordinadores()
        {
            //311882D4-EAD0-4B0B-9C5D-4A434D49D16D	Coordinadores
            return await (from ur in _context.UserRoles
                          join r in _context.Roles on ur.RoleId equals r.Id
                          join u in _context.Users on ur.UserId equals u.Id
                          where r.Id == "311882D4-EAD0-4B0B-9C5D-4A434D49D16D" && u.Activo == true
                          select new UserDto
                          {
                              Id = u.Id,
                              Alias = u.Alias,
                              Email = u.Email,
                              Name = u.FullName
                          }).ToArrayAsync();
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

                    string registroNuevo = JsonConvert.SerializeObject(plantillaCorreo);

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        IdPlantilla = plantillaCorreo.Id,
                        Transaccion = "Creacion",
                        Comentario = "Creacion",
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = "Juan Manuel",
                        UsuarioRol = "Coordinador Admin",
                        RegistroNuevo = registroNuevo
                    };

                    _context.HistoricosPlantilla.Add(historicoPlantilla);
                    _context.SaveChanges();

                    return "Plantilla creada exitosamente";
                }
                else
                {
                    string registroAnterior = JsonConvert.SerializeObject(plantillaCorreo);
                    List<string> listComentario = new List<string>();
                    if (plantillaCorreo.Asunto != request.Asunto)
                    {
                        listComentario.Add("Asunto");
                    }
                    if (plantillaCorreo.Cierre != request.Cierre)
                    {
                        listComentario.Add("Cierre");
                    }
                    if (plantillaCorreo.Estado != request.Estado)
                    {
                        listComentario.Add("Estado");
                    }
                    if (plantillaCorreo.Firmante != request.Firmante)
                    {
                        listComentario.Add("Firmante");
                    }
                    if (plantillaCorreo.Mensaje != request.Mensaje)
                    {
                        listComentario.Add("Mensaje");
                    }
                    if (plantillaCorreo.Nombre != request.Nombre)
                    {
                        listComentario.Add("Nombre");
                    }
                    if (plantillaCorreo.TipoPlantilla != request.TipoPlantilla)
                    {
                        listComentario.Add("Tipo Plantilla");
                    }
                    string comentario = string.Join(", ", listComentario);

                    plantillaCorreo.Asunto = request.Asunto;
                    plantillaCorreo.Cierre = request.Cierre;
                    plantillaCorreo.Estado = request.Estado;
                    plantillaCorreo.Firmante = request.Firmante;
                    plantillaCorreo.Mensaje = request.Mensaje;
                    plantillaCorreo.Nombre = request.Nombre;
                    plantillaCorreo.TipoPlantilla = request.TipoPlantilla;

                    _context.PlantillaCorreos.Update(plantillaCorreo);
                    _context.SaveChanges();

                    string registroNuevo = JsonConvert.SerializeObject(plantillaCorreo);

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        IdPlantilla = plantillaCorreo.Id,
                        Transaccion = "Modificacion",
                        Comentario = comentario,
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = "Juan Manuel",
                        UsuarioRol = "Coordinador Admin",
                        RegistroAnterior = registroAnterior,
                        RegistroNuevo = registroNuevo
                    };

                    _context.HistoricosPlantilla.Add(historicoPlantilla);
                    _context.SaveChanges();

                    return "Plantilla modificada exitosamente";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
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
                    string registroAnterior = JsonConvert.SerializeObject(plantillaCorreo);

                    _context.PlantillaCorreos.Remove(plantillaCorreo);
                    _context.SaveChanges();

                    HistoricoPlantilla historicoPlantilla = new()
                    {
                        IdPlantilla = plantillaCorreo.Id,
                        Transaccion = "Eliminacion",
                        Comentario = "Eliminacion",
                        FechaCreacion = DateTime.Now,
                        UsuarioOrigen = "Juan Manuel",
                        UsuarioRol = "Coordinador Admin",
                        RegistroAnterior = registroAnterior
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

        public PlantillaCorreo ConsultarUnaPlantillasCorreo(long id)
        {
            PlantillaCorreo? plantillaCorreo = (from p in _context.PlantillaCorreos
                                                where p.Id == id
                                                select p).FirstOrDefault();

            return plantillaCorreo;
        }

        public List<HistoricoPlantillaCorreoResponse> HistoricoPlantillaCorreo(long id)
        {
            List<HistoricoPlantillaCorreoResponse> response = (from h in _context.HistoricosPlantilla
                                                               where h.IdPlantilla == id
                                                               select new HistoricoPlantillaCorreoResponse()
                                                               {
                                                                   Id = h.Id.ToString(),
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
                VwExportarDetalleSeguimientoModel? vwSeg = _context.VwExportarDetalleSeguimiento.FirstOrDefault(seg => seg.SeguimientoId == id);
                if (vwSeg != null)
                {
                    List<Seguimiento>? seguimientos = _context.Seguimientos.Where(s => s.NNAId == vwSeg.NNAId).OrderByDescending(x => x.FechaSeguimiento).ToList();

                    string htmlContent = File.ReadAllText("PlantillaExportarSeguimiento.html");
                    htmlContent = htmlContent
                        .Replace("{nombreNNA}", vwSeg.Nombres)
                        .Replace("{fechaHoy}", DateTime.Now.ToString("dd/MM/yyyy"))
                        .Replace("{edadNNA}", vwSeg.Edad)
                        .Replace("{seguimientosRealizados}", vwSeg.SeguimientosRealizados.ToString())
                        .Replace("{diagnosticoNNA}", vwSeg.Diagnostico)
                        .Replace("{seguimientosEnProceso}", vwSeg.SeguimientosEnProceso.ToString())
                        .Replace("{fechaInicioSeguimiento}", vwSeg.FechaSeguimiento)
                        .Replace("{fechaSivigila}", vwSeg.FechaSivigila)
                        .Replace("{sexo}", vwSeg.Sexo)
                        .Replace("{tipoIdentificacion}", vwSeg.TipoIdentificacion)
                        .Replace("{numeroIdentificacion}", vwSeg.NumeroIdentificacion)
                        .Replace("{fechaNacimiento}", vwSeg.FechaNacimiento)
                        .Replace("{paisNacimiento}", vwSeg.Pais)
                        .Replace("{etnia}", vwSeg.Etnia)
                        .Replace("{departamentoNacimiento}", vwSeg.DepartamentoNacimiento)
                        .Replace("{ciudadNacimiento}", vwSeg.CiudadNacimiento)
                        .Replace("{origenReporte}", vwSeg.OrigenReporte)
                        .Replace("{departamentoTratamiento}", vwSeg.DepartamentoTratamiento)
                        .Replace("{estadoIngesoEstrategia}", vwSeg.EstadoIngresoEstrategia)
                        .Replace("{fechaIngresoEstrategia}", vwSeg.FechaIngresoEstrategia)
                        .Replace("{grupoPoblacional}", vwSeg.GrupoPoblacional)
                        .Replace("{semanasGestacion}", "")
                        .Replace("{regimenAfiliacion}", vwSeg.RegimenAfiliacion)
                        .Replace("{asegurador}", vwSeg.Asegurador)
                        .Replace("{ips}", vwSeg.Ips)
                        .Replace("{razonesNoDiagnostico}", vwSeg.RazonesNoDiagnostico)
                        .Replace("{fechaConsulta}", vwSeg.FechaConsulta)
                        .Replace("{fechaDiagnostico}", vwSeg.FechaDiagnostico)
                        .Replace("{fechaInicioTratamiento}", vwSeg.FechaInicioTratamiento)
                        .Replace("{ipsTratamiento}", vwSeg.IpsTratamiento)
                        .Replace("{recaidas}", vwSeg.Recaida)
                        .Replace("{cantidadRecaidas}", vwSeg.CantidadRecaidas.ToString())
                        .Replace("{fechaUltimaRecaida}", vwSeg.FechaUltimaRecaida)
                        .Replace("{procedenciaDepartamento}", vwSeg.ProcedenciaDepartamento)
                        .Replace("{procedenciaMunicipio}", vwSeg.ProcedenciaMunicipio)
                        .Replace("{procedenciaBarrio}", vwSeg.ProcedenciaBarrio)
                        .Replace("{procedenciaArea}", vwSeg.ProcedenciaArea)
                        .Replace("{procedenciaDireccion}", vwSeg.ProcedenciaDireccion)
                        .Replace("{procedenciaEstrato}", vwSeg.ProcedenciaEstrato)
                        .Replace("{procedenciaTelefono}", vwSeg.ProcedenciaTelefono)
                        .Replace("{actualDepartamento}", vwSeg.ActualDepartamento)
                        .Replace("{actualMunicipio}", vwSeg.ActualMunicipio)
                        .Replace("{actualBarrio}", vwSeg.ActualBarrio)
                        .Replace("{actualArea}", vwSeg.ActualArea)
                        .Replace("{actualDireccion}", vwSeg.ActualDireccion)
                        .Replace("{actualEstrato}", vwSeg.ActualEstrato)
                        .Replace("{actualTelefono}", vwSeg.ActualTelefono)
                        .Replace("{requirioTraslado}", vwSeg.RequirioTraslado)
                        .Replace("{capacidadEconomica}", vwSeg.CapacidadEconomica)
                        .Replace("{serviciosSocialesA}", vwSeg.ServiciosSocialesA)
                        .Replace("{oportunidadSSA}", vwSeg.OportunidadSSA)
                        .Replace("{coberturaTrasladoSSA}", vwSeg.CoberturaTrasladoSSA)
                        .Replace("{nombreFundacion}", vwSeg.NombreFundacion)
                        .Replace("{apoyoFundacion}", vwSeg.ApoyoFundacion)
                        .Replace("{sitioResidencia}", vwSeg.SitioResidencia)
                        .Replace("{asumioCostoTraslado}", vwSeg.AsumioCostoTraslado)
                        .Replace("{asumioCostoVivienda}", vwSeg.AsumioCostoVivienda)
                        .Replace("{autorizacionMed}", vwSeg.AutorizacionMed)
                        .Replace("{entregaMedLAP}", vwSeg.EntregaMedLAP)
                        .Replace("{entregaMedNoLAP}", vwSeg.EntregaMedNoLAP)
                        .Replace("{asignacionCitas}", vwSeg.AsignacionCitas)
                        .Replace("{cobroCopagos}", vwSeg.CobroCopagos)
                        .Replace("{autorizacionProc}", vwSeg.AutorizacionProc)
                        .Replace("{remisionIExp}", vwSeg.RemisionIExp)
                        .Replace("{malaAtencionIPS}", vwSeg.MalaAtencionIPS)
                        .Replace("{fallaMipres}", vwSeg.FallaMipres)
                        .Replace("{fallaEapbIps}", vwSeg.FallaEapbIps)
                        .Replace("{transladoInstitucion}", vwSeg.TransladoInstitucion)
                        .Replace("{numeroTraslado}", vwSeg.NumeroTraslado.ToString())
                        .Replace("{ipsTraslado}", vwSeg.IpsTraslado)
                        .Replace("{accionLegal}", vwSeg.AccionLegal)
                        .Replace("{motivoAccionLegal}", vwSeg.MotivoAccionLegal)
                        .Replace("{tipoRecursoAccionLegal}", vwSeg.TipoRecursoAccionLegal)
                        .Replace("{dejoAsistirTratamiento}", vwSeg.DejoAsistirTratamiento)
                        .Replace("{cuantoTiempoTratamiento}", vwSeg.CuantoTiempoTratamiento)
                        .Replace("{causaInasistencia}", vwSeg.CausaInasistencia)
                        .Replace("{otaCausaCual}", vwSeg.OtaCausaCual)
                        .Replace("{estudiando}", vwSeg.Estudiando)
                        .Replace("{dejoAsistirColegio}", vwSeg.DejoAsistirColegio)
                        .Replace("{cuantoTiempoColegio}", vwSeg.CuantoTiempoColegio)
                        .Replace("{informeClaroDiagTrat}", vwSeg.InformeClaroDiagTrat)
                        .Replace("{obsSolicitante}", (seguimientos != null) ? seguimientos.First().ObservacionesSolicitante : "")
                        .Replace("{obsAgente}", (seguimientos != null) ? seguimientos.First().ObservacionAgente : "")
                        ;

                    List<ContactoNNA> contactosNNA = _context.ContactoNNAs.Where(contacto => contacto.NNAId == vwSeg.NNAId).ToList();
                    string htmlContactos = "";
                    string plantillaContactos = File.ReadAllText("PlantillaContactos.html");
                    foreach (var contacto in contactosNNA)
                    {
                        TPParentescos? tPParentescos = _context.TPParentescos.FirstOrDefault(p => p.Id == contacto.ParentescoId);
                        string? parentesco = (tPParentescos == null) ? "" : tPParentescos.Nombre;
                        htmlContactos += plantillaContactos
                            .Replace("{contactoNombre}", contacto.Nombres)
                            .Replace("{contactoParentesco}", parentesco)
                            .Replace("{contactoEmail}", contacto.Email)
                            .Replace("{contactoTelefono}", contacto.Telefonos);
                    }
                    htmlContent = htmlContent.Replace("{contactos}", htmlContactos);

                    string htmlSeguimientos = "";
                    string plantillaSeguimientos = File.ReadAllText("PlantillaSeguimientos.html");
                    foreach (var seguimiento in seguimientos)
                    {
                        htmlSeguimientos += plantillaSeguimientos
                            .Replace("{seguimientoNumero}", seguimiento.Id.ToString())
                            .Replace("{seguimientoFecha}", seguimiento.FechaSeguimiento?.ToString("dd/MM/yyyy"))
                            .Replace("{seguimientoAsunto}", seguimiento.UltimaActuacionAsunto)
                            .Replace("{seguimientoObservacion}", seguimiento.ObservacionAgente)
                            ;
                    }
                    htmlContent = htmlContent.Replace("{tbSeguimientos}", htmlSeguimientos);

                    List<VwExportarDetalleSeguimientoAlertasModel> vwAlertas = _context.VwExportarDetalleSeguimientoAlertas.Where(alerta => alerta.SeguimientoId == id).ToList();
                    string htmlAlertas = "";
                    string plantillaAlertas = File.ReadAllText("PlantillaAlertas.html");
                    foreach (var alerta in vwAlertas)
                    {
                        htmlAlertas += plantillaAlertas
                            .Replace("{alertaNumeroSeguimiento}", alerta.SeguimientoId.ToString())
                            .Replace("{alertaFecha}", alerta.FechaNotificacion)
                            .Replace("{alertaCategoria}", alerta.Categoria)
                            .Replace("{alertaSubcategoria}", alerta.SubCategoriaAlerta)
                            .Replace("{alertaEntidad}", vwSeg.Ips)
                            .Replace("{alertaObservacion}", alerta.Observaciones)
                            .Replace("{alertaEstado}", alerta.Estado)
                            ;
                    }
                    htmlContent = htmlContent.Replace("{tbAlertas}", htmlAlertas);

                    using (var pdfStream = new MemoryStream())
                    {
                        ConverterProperties properties = new ConverterProperties();
                        properties.SetBaseUri("");
                        DefaultFontProvider fontProvider = new DefaultFontProvider(false, true, true);
                        properties.SetFontProvider(fontProvider);
                        PdfWriter writer = new PdfWriter(pdfStream);
                        PdfDocument pdf = new PdfDocument(writer);
                        pdf.SetDefaultPageSize(PageSize.LETTER);
                        // Convertir HTML a PDF usando el MemoryStream
                        HtmlConverter.ConvertToPdf(htmlContent, pdf, properties);

                        // Convertir el MemoryStream a un array de bytes
                        byte[] pdfBytes = pdfStream.ToArray();

                        // Convertir el PDF a Base64
                        var base64Pdf = Convert.ToBase64String(pdfBytes);

                        //File.WriteAllBytes("D:\\Temp\\DetalleSeguimiento.pdf", pdfBytes);

                        // Retornar el PDF en Base64
                        response.Base64 = base64Pdf;
                    }
                }
            }
            catch (PdfException exception)
            {
                Console.WriteLine(exception.ToString());
                response.Nombre = "Ha ocurrido un error - PdfException";
                response.Base64 = exception.ToString();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                response.Nombre = "Ha ocurrido un error - Exception";
                response.Base64 = exception.ToString();
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
