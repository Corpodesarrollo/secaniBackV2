﻿using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Core.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class SeguimientoRepo(ApplicationDbContext context) : ISeguimientoRepo
    {
        private readonly ApplicationDbContext _context = context;

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
                       FechaNotificacion = s.FechaSeguimiento,
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
                    return result.Where(x => x.AsuntoUltimaActuacion.ToLower() == "solicitado por cuidador").ToList();

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
                    SolicitadosPorCuidador = result.Count(x => x.AsuntoUltimaActuacion.Equals("solicitado por cuidador", StringComparison.CurrentCultureIgnoreCase))
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
                                        NombreCompleto = string.Join(" ", n.PrimerNombre, n.SegundoNombre, n.PrimerApellido, n.SegundoApellido),
                                        Diagnostico = diagnostico.Nombre,
                                        FechaNacimiento = n.FechaNacimiento,
                                        FechaIngresoEstrategia = n.FechaIngresoEstrategia,
                                        FechaInicioSeguimiento = (from s in _context.Seguimientos
                                                                  where s.NNAId == id
                                                                  orderby s.Id descending
                                                                  select s.FechaSeguimiento).FirstOrDefault(),
                                        SeguimientosRealizados = (from s in _context.Seguimientos
                                                                  where s.NNAId == id
                                                                  select s).Count()
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

                                                     join alerta in _context.AlertaSeguimientos on un.Id equals alerta.SeguimientoId into alertaGroup
                                                     from subAlerta in alertaGroup.DefaultIfEmpty()

                                                     where un.UsuarioId == UsuarioId
                                                           && un.FechaSeguimiento >= FechaInicial
                                                           && un.FechaSeguimiento <= FechaFinal
                                                           && un.EstadoId != 3
                                                     group subAlerta by new
                                                     {
                                                         un.Id,
                                                         un.NNAId,
                                                         un.FechaSeguimiento,
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
                                                         FechaSeguimiento = g.Key.FechaSeguimiento,
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

            var seguimiento = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimiento == null)
            {
                return -1;
            }

            seguimiento.FechaSeguimiento = request.FechaSeguimiento;

            _context.SaveChanges();
            return 1;
        }

        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            var seguimientoOriginal = _context.Seguimientos.FirstOrDefault(s => s.Id == request.Id);

            if (seguimientoOriginal == null)
            {
                return -1;
            }


            DateTime hoy = DateTime.Now;
            if (seguimientoOriginal.FechaSeguimiento < hoy)
            {
                return -2;
            }

            // Actualizar el EstadoId a cero
            seguimientoOriginal.EstadoId = 3;

            // Guardar los cambios en el seguimiento original
            _context.SaveChanges();

            try
            {

                var nuevoSeguimiento = new Seguimiento
                {
                    NNAId = seguimientoOriginal.NNAId,
                    FechaSeguimiento = seguimientoOriginal.FechaSeguimiento,

                    EstadoId = 1, // Valor inicial para el nuevo seguimiento (TODO: Definir por inexistencia de parametricas )

                    ContactoNNAId = seguimientoOriginal.ContactoNNAId,
                    Telefono = seguimientoOriginal.Telefono,

                    UsuarioId = request.UsuarioId, // Cambiar el valor del UsuarioId

                    SolicitanteId = seguimientoOriginal.SolicitanteId,
                    FechaSolicitud = seguimientoOriginal.FechaSolicitud,
                    TieneDiagnosticos = seguimientoOriginal.TieneDiagnosticos,

                    ObservacionesSolicitante = seguimientoOriginal.ObservacionesSolicitante + " " + request.ObservacionesSolicitante, // Nuevas observaciones

                    DateCreated = DateTime.Now,
                    CreatedByUserId = seguimientoOriginal.CreatedByUserId,
                    IsDeleted = false
                };

                _context.Seguimientos.Add(nuevoSeguimiento);
                _context.SaveChanges();
            }
            catch (Exception)
            {

                return -3;
            }

            return 1;
        }

        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoFestivoResponse> response = (from un in _context.TPFestivos
                                                            where
                                                                  un.Festivo >= FechaInicial
                                                                  && un.Festivo <= FechaFinal

                                                            select new GetSeguimientoFestivoResponse()
                                                            {

                                                                Festivo = un.Festivo,

                                                            }).ToList();



            return response;
        }

        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetSeguimientoHorarioAgenteResponse> response = (from un in _context.HorarioLaboralAgente
                                                                  where
                                                                  un.UserId == UsuarioId
                                                                       && un.Fecha >= FechaInicial
                                                                       && un.Fecha <= FechaFinal

                                                                  select new GetSeguimientoHorarioAgenteResponse()
                                                                  {

                                                                      Fecha = un.Fecha,
                                                                      HoraEntrada = un.HoraEntrada,
                                                                      HoraSalida = un.HoraSalida,

                                                                  }).ToList();



            return response;
        }

        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId)
        {
            var response = (from ur in _context.AspNetUserRoles
                            join r in _context.AspNetRoles on ur.RoleId equals r.Id
                            join u in _context.AspNetUsers on ur.UserId equals u.Id
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
                               NombreAlerta = sca.CategoriaAlertaId + "." + sca.Indicador
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
    }
}
