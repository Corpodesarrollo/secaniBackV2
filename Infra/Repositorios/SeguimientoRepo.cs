using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Utilities;
using iText.Html2pdf;
using iText.Kernel.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class SeguimientoRepo(ApplicationDbContext context, IWebHostEnvironment env) : ISeguimientoRepo
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

        public async Task AsignacionAutomatica()
        {
            var estados = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 15 };

            var seguimientos = await (from seg in _context.Seguimientos
                                      join nna in _context.NNAs on seg.NNAId equals nna.Id
                                      where nna.estadoId.HasValue && estados.Contains(nna.estadoId.Value)
                                      select seg.Id).ToArrayAsync();

            //14CDDEA5-FA06-4331-8359-036E101C5046	Agentes de seguimiento
            var revisores = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   join u in _context.Users on ur.UserId equals u.Id
                                   where r.Id == "14CDDEA5-FA06-4331-8359-036E101C5046"
                                   select u).ToListAsync();

            var usuariosAsignados = new List<UsuarioAsignado>();
            int revisorIndex = 0;

            foreach (var item in seguimientos)
            {
                var revisor = revisores[revisorIndex];
                usuariosAsignados.Add(new UsuarioAsignado
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = DateTime.Now,
                    Observaciones = "Asignación automática",
                    SeguimientoId = item,
                    UsuarioId = revisor.Id
                });

                revisorIndex = (revisorIndex + 1) % revisores.Count;
            }

            _context.UsuarioAsignados.AddRange(usuariosAsignados);
            await _context.SaveChangesAsync();
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
                ExportarDetalleSeguimientoDto? seguimiento = (from seg in _context.Seguimientos
                                                              join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                              join c in _context.CIE10s on nna.DiagnosticoId equals c.Id
                                                              where seg.Id == id
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

                seguimiento.Contactos = (from seg in _context.Seguimientos
                                         join cont in _context.ContactoNNAs on seg.NNAId equals cont.NNAId
                                         where seg.Id == seguimiento.Id
                                         select new ExportarDetalleSeguimientoContactoDto()
                                         {
                                             CorreoElectronico = cont.Email,
                                             Nombre = cont.Nombres,
                                             Parentesco = cont.ParentescoId,
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

                if (seguimiento != null)
                {
                    string htmlContent = "<!DOCTYPE  html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"es\" lang=\"es\">\r\n\t<head>\r\n\t\t<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>\r\n\t\t<title>94c51429-f73e-4fc3-831b-4569c1da8b03</title>\r\n\t\t<meta name=\"author\" content=\"CAROL ANNE  RAMIREZ CHAPARRO\"/>\r\n\t\t<style type=\"text/css\"> * {margin:0; padding:0; text-indent:0; }\r\n .s1 { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: bold; text-decoration: none; font-size: 10pt; }\r\n .s2 { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 10pt; }\r\n p { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: bold; text-decoration: none; font-size: 8pt; margin:0pt; }\r\n .s4 { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 8pt; }\r\n .s5 { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: bold; text-decoration: none; font-size: 8pt; }\r\n h1 { color: #0D0D0D; font-family:Verdana, sans-serif; font-style: normal; font-weight: bold; text-decoration: none; font-size: 8pt; }\r\n .s6 { color: #0D0D0D; font-family:Verdana, sans-serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 8pt; }\r\n .s7 { color: #0D0D0D; font-family:Arial, sans-serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 8pt; }\r\n .s8 { color: black; font-family:Verdana, sans-serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 8pt; }\r\n li {display: block; }\r\n #l1 {padding-left: 0pt; }\r\n #l1> li>*:first-child:before {content: \" \"; color: black; font-family:Symbol, serif; font-style: normal; font-weight: normal; text-decoration: none; font-size: 8pt; }\r\n table, tbody {vertical-align: top; overflow: visible; }\r\n</style>\r\n\t</head>\r\n\t<body>\r\n\t\t<p style=\"padding-top: 1pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<table style=\"border-collapse:collapse;margin-left:7.9pt\" cellspacing=\"0\">\r\n\t\t\t<tr style=\"height:50pt\">\r\n\t\t\t\t<td style=\"width:263pt\">\r\n\t\t\t\t\t<p class=\"s1\" style=\"padding-left: 2pt;text-indent: 0pt;line-height: 12pt;text-align: left;\">{NombreNNA}</p>\r\n\t\t\t\t\t<p class=\"s2\" style=\"padding-left: 2pt;text-indent: 0pt;line-height: 12pt;text-align: left;\">Edad: {edadNNA}</p>\r\n\t\t\t\t\t<p class=\"s2\" style=\"padding-left: 2pt;text-indent: 0pt;line-height: 12pt;text-align: left;\">Diagnóstico:{DiagnosticoNNA}</p>\r\n\t\t\t\t\t<p class=\"s2\" style=\"padding-left: 2pt;text-indent: 0pt;line-height: 11pt;text-align: left;\">Fecha inicio seguimiento: {fechaInicioSeguimiento}</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:227pt\">\r\n\t\t\t\t\t<p class=\"s2\" style=\"padding-left: 68pt;padding-right: 2pt;text-indent: -7pt;text-align: right;\">Fecha generación: {FechaHoy}<br/> <u>\r\n\t\t\t\t\t\t\t<b>{seguimientosRealizados} seguimientos realizados</b><br/>\r\n\t\t\t\t\t\t</u>\r\n\t\t\t\t\t\t<b> </b>\r\n\t\t\t\t\t\t<u>\r\n\t\t\t\t\t\t\t<b>{SeguimientoId} Seguimiento en proceso</b>\r\n\t\t\t\t\t\t</u>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<p style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Datos básicos</p>\r\n\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t<tr style=\"height:18pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Fecha de notificación del SIVIGILA</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">{fechaSivigila}</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:18pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Sexo</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">{sexo}</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Tipo de identificación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{tipoIdentificacion}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Número de identificación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{numeroIdentificacion}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha de nacimiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{fechaNacimiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">País de nacimiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{paisNacimiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Etnia</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{etnia}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Departamento de nacimiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{departamentoNacimiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Ciudad de nacimiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{ciudadNacimiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Origen del reporte</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{origenReporte}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">Departamento donde actualmente recibe el tratamiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{departamentoTratamiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Estado de ingreso a la estrategia</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{departamentoTratamiento}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha de ingreso a la estrategia</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{fechaIngresoEstrategia}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Grupo poblacional</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{grupoPoblacional}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Semanas de Gestación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{semanasGestacion}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Régimen de afiliación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{regimenAfiliacion}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Asegurador</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{asegurador}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">IPS</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t{Ips}\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<p style=\"padding-left: 40pt;text-indent: 0pt;text-align: left;\">Contactos</p>\r\n\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<table style=\"border-collapse:collapse;margin-left:40.434pt\" cellspacing=\"0\">\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:106pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s5\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Nombre</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:100pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s5\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Parentesco</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:99pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s5\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Correo electrónico</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:156pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s5\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Teléfono</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:106pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:100pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:99pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:156pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<p style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Información del tratamiento</p>\r\n\t\t<ul id=\"l1\">\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-top: 8pt;padding-left: 40pt;text-indent: -17pt;text-align: left;\">Diagnóstico y tratamiento</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;padding-right: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">Razones por las cuales el menor no tiene diagnóstico.</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{razonesNoTratamiento}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:11pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Diagnóstico</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{DiagnosticoNNA}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:11pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha de consulta</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{fechaConsulta}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:11pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha de diagnóstico</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{fechaDiagnostico}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:11pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha de inicio de tratamiento</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{fechaInicioTratamiento}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:22pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Nombre de la Institución en la que recibe</p>\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">tratamiento actualmente (IPS)</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{IpsTratamiento}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 40pt;text-indent: -17pt;text-align: left;\">Recaídas</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Ha tenido recaídas</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{tieneRecaidas}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Número de recaidas</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{numeroRecaidas}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:221pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha última recaída</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:275pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{fechaUltimaRecaida}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 8pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 40pt;text-indent: -17pt;text-align: left;\">Residencia y traslados</p>\r\n\t\t\t\t<p style=\"padding-top: 3pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<h1 style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Residencia de procedencia/ocurrencia</h1>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Departamento</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{departamentoResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Municipio</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{municipioResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Barrio</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{barrioResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Área</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{areaResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Dirección</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{direccionResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Estrato</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{estratoResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Teléfono</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{telefonoResidencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Traslado</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:30pt\">\r\n\t\t\t\t\t\t<td style=\"width:220pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-top: 5pt;padding-left: 5pt;padding-right: 27pt;text-indent: 0pt;text-align: left;\">Requirió trasladarse de ciudad para acceder al tratamiento</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:276pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{requirioTrasladarse}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 8pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<h1 style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Residencia actual</h1>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Departamento</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{departamentoResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Municipio</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{municipioResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Barrio</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{barrioResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Área</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{areaResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Dirección</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{direccionResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Estrato</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{estratoResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Teléfono</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{telefonoResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 6pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">Cuenta con la capacidad económica para asumir el traslado del menor</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{capacidadEconomicaTraslado}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;padding-right: 9pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">La EAPB le ha suministrado servicios sociales de apoyo para el traslado</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{apoyoTraslado}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">¿Los servicios sociales de apoyo fueron entregados con oportunidad?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{apoyoOportuno}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">¿Los servicios sociales de apoyo logran dar cobertura al traslado del menor?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{coberturaServicioSocial}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Nombre de la fundación</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{nombreFundacion}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Apoyo recibido por la fundación</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{apoyoFundacion}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">El sitio de residencia actual es</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{tipoResidenciaActual}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Quién asumió los costos del traslado?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{asumioCostosTraslado}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Quién asumió los costos de la vivienda?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{asumioCostosVivienda}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 40pt;text-indent: -17pt;text-align: left;\">Dificultades y traslados hospitalarios</p>\r\n\t\t\t\t<h1 style=\"padding-top: 8pt;padding-left: 5pt;text-indent: 0pt;text-align: left;\">¿Ha presentado dificultades en los siguientes procesos?</h1>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Autorización de medicamentos</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{dificultadAutorizacionMedicamentos}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Entrega de medicamentos LAP</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{dificultadEntregaMedicamentosLAP}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Entrega de medicamentos No LAP</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{dificultadEntregaMedicamentosNoLAP}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Asignación de citas</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{dificultadAsignacionCitas}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Le han cobrado copagos o cuotas moderadoras</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{HanCobradoCopago}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Autorización de procedimientos</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{AutorizacionProcedimiento}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Remisión a instituciones especializadas</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{remisionEspecialista}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Mala atención en la IPS</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">{MalaAtencionIps} {cualIps}</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Falla en MIPRES</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{FallaMipres}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Falla en convenio entre la EAPB e IPS tratante</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{fallaConvenio}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Traslado de institución</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Ha sido trasladado de institución?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{HaTrasladado}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Número de traslados</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{numeroTraslados}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">IPS</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{ips}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:20pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">¿Ha tenido que recurrir a algún tipo de acción legal para acceder a la atención?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{haRecurridoAccionLegal}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Motivo</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{Motivo}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Tipo de recurso</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{tipoRecurso}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 40pt;text-indent: -17pt;text-align: left;\">Adherencia al tratamiento y calendario escolar</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Ha dejado de asistir al tratamiento?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{haDejadoTratamiento}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Por cuánto tiempo ha dejado de asistir?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">{tiempoInasistenciaTratamiento}</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Causas de inasistencia</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{causaInasistencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Otra ¿Cuál?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{CualOtraCausaInasistencia}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s7\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Está estudiando actualmente?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{estudiaActualmente}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s7\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿En algún momento ha dejado de asistir al colegio?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{haDejadoColegio}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s7\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Por cuánto tiempo ha dejado de asistir al colegio?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s6\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">{tiempoInasistenciaColegio</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t\t<tr style=\"height:19pt\">\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p class=\"s7\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">¿Considera que la IPS y/o el médico le han informado de manera</p>\r\n\t\t\t\t\t\t\t<p class=\"s7\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 8pt;text-align: left;\">clara y completa sobre el diagnóstico y el tratamiento del NNA?</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t<td style=\"width:252pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t\t\t{ipsClara}\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t\t<p style=\"padding-top: 9pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 40pt;text-indent: -17pt;text-align: left;\">Observaciones</p>\r\n\t\t\t\t<p class=\"s8\" style=\"padding-top: 8pt;padding-left: 5pt;text-indent: 0pt;text-align: left;\">Observaciones diligenciadas en el último seguimiento.</p>\r\n\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t<br/>\r\n\t\t\t\t</p>\r\n\t\t\t</li>\r\n\t\t\t<li data-list-text=\"\">\r\n\t\t\t\t<p style=\"padding-left: 5pt;text-indent: 18pt;line-height: 189%;text-align: left;\">Trazabilidad Seguimientos</p>\r\n\t\t\t</li>\r\n\t\t</ul>\r\n\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">No.</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Fecha</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:117pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Asunto</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:269pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 9pt;text-align: left;\">Observación</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:55pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:117pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:269pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t\t<p style=\"padding-top: 8pt;text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<p style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Alertas</p>\r\n\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t<br/>\r\n\t\t</p>\r\n\t\t<table style=\"border-collapse:collapse;margin-left:5.25pt\" cellspacing=\"0\">\r\n\t\t\t<tr style=\"height:39pt\">\r\n\t\t\t\t<td style=\"width:60pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;padding-right: 4pt;text-indent: 0pt;text-align: left;\">No. seguimiento</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:56pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;padding-right: 4pt;text-indent: 0pt;text-align: left;\">Fecha notificación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:50pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Categoría</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:63pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Subcategoría</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:69pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Entidad(es) sobre la(s)</p>\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;line-height: 10pt;text-align: left;\">que se genera alerta</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:134pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Observación</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:64pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p class=\"s4\" style=\"padding-left: 5pt;text-indent: 0pt;text-align: left;\">Estado</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr style=\"height:10pt\">\r\n\t\t\t\t<td style=\"width:60pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:56pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:50pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:63pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:69pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:134pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t\t<td style=\"width:64pt;border-top-style:solid;border-top-width:1pt;border-left-style:solid;border-left-width:1pt;border-bottom-style:solid;border-bottom-width:1pt;border-right-style:solid;border-right-width:1pt\">\r\n\t\t\t\t\t<p style=\"text-indent: 0pt;text-align: left;\">\r\n\t\t\t\t\t\t<br/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t</body>\r\n</html>\r\n";

                    htmlContent = htmlContent.Replace("{NombreNNA}", seguimiento.Nombre)
                        .Replace("{FechaHoy}", DateTime.Now.ToString("dd/MM/yyyy"))
                        .Replace("{edadNNA}", edad.ToString())
                        .Replace("{DiagnosticoNNA}", seguimiento.Diagnostico)
                        .Replace("{fechaInicioSeguimiento}", seguimiento.FechaSeguimiento == null ? "" : seguimiento.FechaSeguimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaSivigila}", seguimiento.FechaSeguimiento == null ? "" : seguimiento.FechaSeguimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{sexo}", seguimiento.Sexo)
                        .Replace("{tipoIdentificacion}", seguimiento.TipoIdentificacion)
                        .Replace("{numeroIdentificacion}", seguimiento.NumeroIdentificacion)
                        .Replace("{seguimientosRealizados}", seguimiento.CantidadSegumientos.ToString())
                        .Replace("{SeguimientoId}", seguimiento.Id.ToString())
                        .Replace("{fechaNacimiento}", seguimiento.FechaNacimiento == null ? "" : seguimiento.FechaNacimiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{paisNacimiento}", seguimiento.PaisNacimiento)
                        .Replace("{etnia}", seguimiento.Etnia)
                        .Replace("{departamentoNacimiento}", seguimiento.DepartamentoNacimiento)
                        .Replace("{ciudadNacimiento}", seguimiento.CiudadNacimiento)
                        .Replace("{origenReporte}", seguimiento.OrigenReporte.ToString())
                        .Replace("{departamentoTratamiento}", seguimiento.DepartamentoTratamiento)
                        .Replace("{fechaIngresoEstrategia}", seguimiento.FechaIngreso == null ? "" : seguimiento.FechaIngreso.Value.ToString("dd/MM/yyyy"))
                        .Replace("{grupoPoblacional}", seguimiento.GrupoPoblacional)
                        .Replace("{semanasGestacion}", seguimiento.SemanasGestacion.ToString())
                        .Replace("{regimenAfiliacion}", seguimiento.RegimenAfiliacion)
                        .Replace("{asegurador}", seguimiento.Asegurador.ToString())
                        .Replace("{Ips}", seguimiento.Ips.ToString())
                        .Replace("{razonesNoTratamiento}", seguimiento.razonesNoTratamiento)
                        .Replace("{fechaConsulta}", seguimiento.fechaConsulta == null ? "" : seguimiento.fechaConsulta.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaDiagnostico}", seguimiento.fechaDiagnostico == null ? "" : seguimiento.fechaDiagnostico.Value.ToString("dd/MM/yyyy"))
                        .Replace("{fechaInicioTratamiento}", seguimiento.fechaInicioTratamiento == null ? "" : seguimiento.fechaInicioTratamiento.Value.ToString("dd/MM/yyyy"))
                        .Replace("{IpsTratamiento}", seguimiento.IpsTratamiento)
                        .Replace("{tieneRecaidas}", seguimiento.tieneRecaidas == null ? "NA" : seguimiento.tieneRecaidas.Value ? "SI" : "NO")
                        .Replace("{numeroRecaidas}", seguimiento.numeroRecaidas == null ? "0" : seguimiento.numeroRecaidas.ToString())
                        .Replace("{fechaUltimaRecaida}", seguimiento.fechaUltimaRecaida == null ? "" : seguimiento.fechaUltimaRecaida.Value.ToString("dd/MM/yyyy"))
                        .Replace("{departamentoResidencia}", seguimiento.departamentoResidencia)
                        .Replace("{municipioResidencia}", seguimiento.municipioResidencia)
                        .Replace("{barrioResidencia}", seguimiento.barrioResidencia)
                        .Replace("{areaResidencia}", seguimiento.areaResidencia)
                        .Replace("{direccionResidencia}", seguimiento.direccionResidencia)
                        .Replace("{estratoResidencia}", seguimiento.estratoResidencia)
                        .Replace("{telefonoResidencia}", seguimiento.telefonoResidencia)
                        .Replace("{requirioTrasladarse}", seguimiento.requirioTrasladarse ? "SI" : "NO")
                        .Replace("{departamentoResidenciaActual}", seguimiento.departamentoResidenciaActual)
                        .Replace("{municipioResidenciaActual}", seguimiento.municipioResidenciaActual)
                        .Replace("{barrioResidenciaActual}", seguimiento.barrioResidenciaActual)
                        .Replace("{areaResidenciaActual}", seguimiento.areaResidenciaActual)
                        .Replace("{direccionResidenciaActual}", seguimiento.direccionResidenciaActual)
                        .Replace("{estratoResidenciaActual}", seguimiento.estratoResidenciaActual)
                        .Replace("{telefonoResidenciaActual}", seguimiento.telefonoResidencia)
                        .Replace("{capacidadEconomicaTraslado}", seguimiento.capacidadEconomicaTraslado == null ? "NA" : seguimiento.capacidadEconomicaTraslado.Value ? "SI" : "NO")
                        .Replace("{apoyoTraslado}", seguimiento.apoyoTraslado == null ? "NA" : seguimiento.apoyoTraslado.Value ? "SI" : "NO")
                        .Replace("{apoyoOportuno}", seguimiento.apoyoOportuno == null ? "NA" : seguimiento.apoyoOportuno.Value ? "SI" : "NO")
                        .Replace("{coberturaServicioSocial}", seguimiento.coberturaServicioSocial == null ? "NA" : seguimiento.coberturaServicioSocial.Value ? "SI" : "NO")
                        .Replace("{nombreFundacion}", seguimiento.nombreFundacion)
                        .Replace("{apoyoFundacion}", seguimiento.apoyoFundacion)
                        .Replace("{tipoResidenciaActual}", seguimiento.tipoResidenciaActual)
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
                              TipoIdentificacion = n.TipoIdentificacionId,
                              NumeroIdentificacion = n.NumeroIdentificacion,
                              Parentesco = p != null ? p.Nombre : "",
                              Diagnostico = d != null ? d.Nombre : "",
                              Estado = new TPEstadoNNADto()
                              {
                                  Nombre = e.Nombre,
                                  Descripcion = e.Descripcion,
                                  ColorBG = e.ColorBG,
                                  ColorText = e.ColorText
                              },
                              AsuntoUltimaActuacion = s.UltimaActuacionAsunto,
                              FechaUltimaActuacion = s.UltimaActuacionFecha
                          }).ToArrayAsync();
        }
    }
}
