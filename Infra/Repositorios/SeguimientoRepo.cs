using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PuppeteerSharp.Cdp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Buffers.Text;
using Microsoft.AspNetCore.Hosting;

namespace Infra.Repositorios
{
    public class SeguimientoRepo(ApplicationDbContext context, IWebHostEnvironment env) : ISeguimientoRepo
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        private IQueryable<SeguimientoDto> GetSelect(string id)
        {
            return from s in _context.Seguimientos
                   join e in _context.TPEstadoNNA on s.EstadoId equals e.Id
                   join n in _context.NNAs on s.NNAId equals n.Id
                   where s.UsuarioId == id
                   select new SeguimientoDto()
                   {
                       Id = s.Id,
                       NoCaso = n.Id,
                       PrimerNombre = n.PrimerNombre,
                       SegundoNombre = n.SegundoNombre,
                       PrimerApellido = n.PrimerApellido,
                       SegundoApellido = n.SegundoApellido,
                       FechaNotificacion = n.FechaNotificacionSIVIGILA,
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
        }

        public async Task<List<SeguimientoDto>> GetAllByIdUser(string id, int filtro)
        {
            try
            {
                var query = GetSelect(id);
                var result = await query.ToListAsync();

                if (filtro == 1) //hoy
                    return result.Where(x => x.FechaNotificacion?.Date == DateTime.Now.Date).ToList();

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
                    Hoy = result.Where(x => x.FechaNotificacion?.Date == DateTime.Now.Date).Count(),
                    ConAlerta = result.Where(x => x.Alertas.Count > 0).Count(),
                    SolicitadosPorCuidador = result.Count(x => x.AsuntoUltimaActuacion?.ToLower() == "solicitado por cuidador")
                };
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
                                                         FechaSolicitud = g.Key.FechaSolicitud,
                                                         TieneDiagnosticos = g.Key.TieneDiagnosticos,
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

        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA)
        {
            List<SeguimientoNNAResponse> seguimientos = (from seg in _context.Seguimientos
                                                         join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                         where seg.NNAId == idNNA
                                                         select new SeguimientoNNAResponse()
                                                         {
                                                             FechaNotificacion = seg.FechaSolicitud,
                                                             FechaSeguimiento = seg.UltimaActuacionFecha,
                                                             IdSeguimiento = seg.Id,
                                                             Asunto = seg.UltimaActuacionAsunto,
                                                             Observacion = seg.ObservacionesSolicitante,
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
                               UltimaFechaSeguimiento = alert.UltimaFechaSeguimiento,
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

        public string SetSeguimiento(SetSeguimientoRequest request)
        {
            try
            {
                Seguimiento seguimiento = new();
                seguimiento.NNAId = request.IdNNA;
                seguimiento.FechaSeguimiento = request.FechaSeguimiento;
                seguimiento.EstadoId = request.IdEstado;
                seguimiento.ContactoNNAId = request.IdContactoNNA;
                seguimiento.Telefono = request.Telefono;
                seguimiento.UsuarioId = request.IdUsuario;
                seguimiento.SolicitanteId = request.IdSolicitante;
                seguimiento.ObservacionesSolicitante = request.ObservacionSolicitante;
                seguimiento.CreatedByUserId = request.IdUsuarioCreacion;
                seguimiento.DateCreated = DateTime.Now;

                _context.Seguimientos.Add(seguimiento);
                _context.SaveChanges();

                return "Segumiento almacenado correctamente";
            }
            catch (Exception)
            {
                return "Existe una error al almacenar el segumiento";
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

            _context.SaveChanges();
            return 1;
        }

        public void AsignacionAutomatica()
        {
            List<ConsultaCasosAbiertosResponse> lista;
            List<int> estados = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 15, 16 };
            UsuarioAsignado usuarioAsignado;
            List<UsuarioAsignado> usuarios = new List<UsuarioAsignado>();

            lista = (from seg in _context.Seguimientos
                     join nna in _context.NNAs on seg.NNAId equals nna.Id
                     where nna.estadoId.HasValue && estados.Contains(nna.estadoId.Value)
                     select new ConsultaCasosAbiertosResponse()
                     {
                         AsuntoUltimaActuacion = seg.UltimaActuacionAsunto,
                         Estado = seg.EstadoId,
                         FechaNotificacion = seg.FechaSolicitud,
                         FechaUltimaActuacion = seg.UltimaActuacionFecha,
                         Alertas = new List<AlertaSeguimientoResponse>(),
                         SeguimientoId = seg.Id
                     }).ToList();

            List<ApplicationUser> revisores = (from us in _context.Users
                                               select us).ToList();

            foreach (ConsultaCasosAbiertosResponse r in lista)
            {
                usuarioAsignado = new UsuarioAsignado()
                {
                    Activo = true,
                    DateCreated = DateTime.Now,
                    FechaAsignacion = DateTime.Now,
                    Observaciones = "Asignacion automatica",
                    SeguimientoId = r.SeguimientoId,
                };
                usuarios.Add(usuarioAsignado);
            }

            this.AsignarUsuarios(revisores, usuarios);

            _context.UsuarioAsignados.AddRange(usuarios);
            _context.SaveChanges();
        }

        private void AsignarUsuarios(List<ApplicationUser> usuarios, List<UsuarioAsignado> solicitudes)
        {
            int usuarioIndex = 0;
            int totalUsuarios = usuarios.Count;

            foreach (var solicitud in solicitudes)
            {
                solicitud.UsuarioId = usuarios[usuarioIndex].Id;  // Asignar el usuario
                usuarioIndex = (usuarioIndex + 1) % totalUsuarios;  // Reinicia el índice si se alcanzan todos los usuarios
            }
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

                    HistoricoPlantilla historicoPlantilla = new HistoricoPlantilla()
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

                    HistoricoPlantilla historicoPlantilla = new HistoricoPlantilla()
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
            catch (Exception e)
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

                    HistoricoPlantilla historicoPlantilla = new HistoricoPlantilla()
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

        public ExportarDetalleSeguimientoResponse ExportarDetalleSeguimiento(long id)
        {
            ExportarDetalleSeguimientoResponse response = new ExportarDetalleSeguimientoResponse();
            try
            {
                ExportarDetalleSeguimientoDto? seguimiento = (from seg in _context.Seguimientos
                                                             join nna in _context.NNAs on seg.NNAId equals nna.Id
                                                              join c in _context.CIE10s on nna.DiagnosticoId equals c.Id
                                                              where seg.Id == id
                                                              select new ExportarDetalleSeguimientoDto()
                                                               {
                                                                   Nombre = nna.PrimerNombre+" "+nna.SegundoApellido+" "+nna.PrimerApellido+" "+nna.SegundoApellido,
                                                                   FechaNacimiento = nna.FechaNacimiento,
                                                                   Diagnostico = c.Nombre,
                                                                   FechaSeguimiento = seg.FechaSeguimiento,
                                                                   Id = seg.Id
                                                               }).FirstOrDefault();

                int edad = 0;
                if (seguimiento.FechaNacimiento != null) {
                    edad = DateTime.Now.Year - seguimiento.FechaNacimiento.Value.Year;

                    // Ajustar si la fecha de inicio no ha cumplido el mismo día/mes en el año final
                    if (DateTime.Now < seguimiento.FechaNacimiento.Value.AddYears(edad))
                    {
                        edad--;
                    }
                }

                if(seguimiento!= null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        // Crear un documento PDF
                        PdfDocument document = new PdfDocument();
                        document.Info.Title = "Detalle de Seguimiento";

                        // Crear una página
                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        // Definir fuentes con estilos (Regular y Bold)
                        XFont titleFont = new XFont("Arial", 16);  // Para el título
                        XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold); // Para encabezados en negrita
                        XFont textFont = new XFont("Arial", 10); // Para el texto regular
                        XFont headerSectionFont = new XFont("Arial", 9, XFontStyleEx.Bold); // Para encabezados en negrita

                        // Margen superior
                        double yPoint = 40;

                        // Incluir imagen 1 desde wwwroot (por ejemplo, logo o imagen de cabecera)
                        string imagePath1 = Path.Combine(_env.WebRootPath, "secani.png");
                        if (System.IO.File.Exists(imagePath1))
                        {
                            XImage image1 = XImage.FromFile(imagePath1);
                            gfx.DrawImage(image1, 50, 50, 250, 35); // Ajustar la posición y el tamaño
                            yPoint += 70; // Ajustar el espacio después de la imagen
                        }

                        // Incluir imagen 2 desde wwwroot (otra imagen relacionada con el seguimiento)
                        string imagePath2 = Path.Combine(_env.WebRootPath, "minsalud.png");
                        if (File.Exists(imagePath2))
                        {
                            XImage image2 = XImage.FromFile(imagePath2);
                            gfx.DrawImage(image2, page.Width-100, 45, 50, 50); // Ajustar la posición y el tamaño
                        }

                        // Información personal (datos básicos)
                        gfx.DrawString(seguimiento.Nombre, headerFont, XBrushes.Black, new XPoint(50, yPoint));
                        gfx.DrawString("Fecha generación: " + DateTime.Now.ToString("dd/MM/yyyy"), textFont, XBrushes.Black, new XPoint(400, yPoint));
                        yPoint += 12;
                        gfx.DrawString("Edad: "+(edad==0?"":edad), textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 12;
                        gfx.DrawString("Diagnóstico: "+seguimiento.Diagnostico, textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 12;
                        gfx.DrawString("Fecha inicio seguimiento: "+(seguimiento.FechaSeguimiento==null?"":seguimiento.FechaSeguimiento.Value.ToString("dd/MM/yyyy")), textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 50;

                        // Datos Básicos
                        gfx.DrawString("Datos básicos", headerSectionFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        double xPoint = 50;
                        double sectionHeight = 12;
                        // Definir un borde
                        XPen borderPen = new XPen(XColors.Black, 1); // Línea negra de 1 punto
                        gfx.DrawRectangle(borderPen, xPoint - 10, yPoint - 10, (page.Width - 50)/2, sectionHeight);
                        gfx.DrawRectangle(borderPen, page.Width/2, yPoint - 10, (page.Width - 50) / 2, sectionHeight);
                        gfx.DrawString("Fecha de notificación del SIVIGILA: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Sexo: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Tipo de identificación: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Número de identificación: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Fecha de nacimiento: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 30;

                        // Información del tratamiento
                        gfx.DrawString("Información del Tratamiento", headerFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Diagnóstico: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Fecha de consulta: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Fecha de diagnóstico: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 20;
                        gfx.DrawString("Fecha de inicio de tratamiento: ", textFont, XBrushes.Black, new XPoint(50, yPoint));
                        yPoint += 30;

                        // Guardar el documento en el MemoryStream
                        document.Save(memoryStream, false);

                        // Convertir el MemoryStream a un arreglo de bytes
                        byte[] pdfBytes = memoryStream.ToArray();

                        // Convertir el arreglo de bytes a una cadena Base64

                        response.Base64 = Convert.ToBase64String(pdfBytes);
                        response.Nombre = "Detalle de seguimiento " + seguimiento.Id+".pdf";

                        string outputPath = "C:\\Users\\Giroco\\Documents\\DetalleSeguimiento.pdf"; // Ruta de salida para el archivo PDF
                        using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            memoryStream.WriteTo(fileStream);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                response.Nombre = "Ha ocurrido un error";
            }

            return response;
        }
    }
}
