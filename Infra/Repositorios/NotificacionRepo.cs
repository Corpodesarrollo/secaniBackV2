using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Infra.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class NotificacionRepo : INotificacionRepo
    {
        private readonly ApplicationDbContext _context;

        public NotificacionRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<GetNotificacionResponse> GetNotificacionUsuario(string AgenteDestinoId)
        {
            List<GetNotificacionResponse> response = (from un in _context.NotificacionesUsuarios
                                                      join uDestino in _context.AspNetUsers on un.AgenteDestinoId equals uDestino.Id
                                                      join uOrigen in _context.AspNetUsers on un.AgenteOrigenId equals uOrigen.Id
                                                      where un.AgenteDestinoId == AgenteDestinoId && !un.IsDeleted
                                                      select new GetNotificacionResponse()
                                                      {
                                                          IdNotificacion = un.Id,
                                                          TextoNotificacion = string.Join("", "El Agente de seguimiento ", uOrigen.FullName ?? string.Empty,
                                                          " le ha asignado el caso No. ", un.SeguimientoId.ToString() ?? "N/A"),
                                                          FechaNotificacion = un.FechaNotificacion,
                                                          URLNotificacion = un.Url == null ? "" : un.Url
                                                      }).ToList();

            return response;
        }

        public int GetNumeroNotificacionUsuario(string AgenteDestinoId)
        {
            List<GetNotificacionResponse> response = (from un in _context.NotificacionesUsuarios
                                                      join uDestino in _context.AspNetUsers on un.AgenteDestinoId equals uDestino.Id
                                                      join uOrigen in _context.AspNetUsers on un.AgenteDestinoId equals uOrigen.Id
                                                      where un.AgenteDestinoId == AgenteDestinoId && !un.IsDeleted
                                                      select new GetNotificacionResponse()
                                                      {
                                                          TextoNotificacion = string.Join("", "El Agente de seguimiento ", uOrigen.FullName,
                                                          " le ha asignado el caso No. ", un.SeguimientoId)
                                                      }).ToList();

            return response.Count;
        }

        public string GenerarOficioNotificacion(OficioNotificacionRequest request)
        {
            NotificacionEntidad? notificacionEntidad = (from ne in _context.NotificacionesEntidad
                                                        where ne.Id == request.Id
                                                        select ne).FirstOrDefault();

            Entidad? entidad = (from ent in _context.Entidades
                                where ent.Id == request.IdEntidad
                                select ent).FirstOrDefault();

            AlertaSeguimiento? alerta = (from als in _context.AlertaSeguimientos
                                         where als.Id == request.IdAlertaSeguimiento
                                         select als).FirstOrDefault();

            NNAs? nna = (from Tnna in _context.NNAs
                         where Tnna.Id == request.IdNNA
                         select Tnna).FirstOrDefault();

            AspNetUsers? user = (from us in _context.AspNetUsers
                                 where us.UserName == request.UserName
                                 select us).FirstOrDefault();

            if (user == null)
            {
                return "El usuario no existe";
            }


            if (entidad == null)
            {
                return "La entidad no existe";
            }

            if (alerta == null)
            {
                return "La alerta no existe";
            }

            if (nna == null)
            {
                return "el NNA no existe";
            }

            if (notificacionEntidad == null)
            {
                notificacionEntidad = new NotificacionEntidad();
            }

            notificacionEntidad.EntidadId = request.IdEntidad;
            notificacionEntidad.Entidad = entidad;
            notificacionEntidad.Ciudad = request.Ciudad;
            notificacionEntidad.AlertaSeguimiento = alerta;
            notificacionEntidad.Asunto = request.Asunto;
            notificacionEntidad.Cierre = request.Cierre;
            notificacionEntidad.CiudadEnvio = request.CiudadEnvio;
            notificacionEntidad.FechaEnvio = request.FechaEnvio;
            notificacionEntidad.Membrete = request.Membrete;
            notificacionEntidad.Ciudad = request.Ciudad;
            notificacionEntidad.Mensaje = request.Mensaje;
            notificacionEntidad.Comentario = request.Comentario;
            notificacionEntidad.NNAs = nna;
            notificacionEntidad.NNAId = nna.Id;
            notificacionEntidad.Firmajpg = request.FirmaJpg;

            if (notificacionEntidad.Id == 0)
            {
                notificacionEntidad.CreatedByUserId = user.Id;
                notificacionEntidad.DateCreated = DateTime.Now;
            }
            else
            {
                notificacionEntidad.UpdatedByUserId = user.Id;
                notificacionEntidad.DateUpdated = DateTime.Now;
            }

            _context.NotificacionesEntidad.Add(notificacionEntidad);
            _context.SaveChanges();


            return "Oficio creado correctamente";
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


        public List<GetNotificacionesEntidadResponse> RepoNotificacionEntidadCasos(int entidadId, int alertaSeguimientoId, int nnaId)
        {
            List<GetNotificacionesEntidadResponse> notificacionEntidad = (from ne in _context.NotificacionesEntidad
                                                                          where ne.EntidadId == entidadId
                                                                          && ne.AlertaSeguimientoId == alertaSeguimientoId
                                                                          && ne.NNAId == nnaId
                
                                                                          select new GetNotificacionesEntidadResponse()
                                                                          {
                                                                              EntidadId = ne.EntidadId,
                                                                            
                                                                              CiudadEnvio = ne.CiudadEnvio,
                                                                              FechaEnvio = ne.FechaEnvio,
                                                                              AlertaSeguimientoId = ne.AlertaSeguimientoId,
                                                                              NNAId = ne.NNAId,
                                                                              Ciudad = ne.Ciudad,
                                                                              EmailConfigurationId = ne.EmailConfigurationId,
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


        public List<GetListaCasosResponse> RepoListaCasosNotificacion( int eapbId, int epsId)
        {
            List<GetListaCasosResponse> listaCasos =  (from n in _context.NNAs
                                                         join s in _context.Seguimientos on n.Id equals s.NNAId
                                                         join a in _context.AlertaSeguimientos on s.Id equals a.SeguimientoId
                                                         
                                                       where n.EAPBId == eapbId || n.EPSId == epsId
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



    }
}
