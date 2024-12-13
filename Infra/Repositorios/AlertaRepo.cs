using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Request;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories
{
    public class AlertaRepo : IAlertaRepo
    {
        private readonly ApplicationDbContext _context;

        public AlertaRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public string CrearAlertaSeguimiento(CrearAlertaSeguimientoRequest request)
        {
            try
            {
                ApplicationUser? user = (from users in _context.Users
                                         where users.UserName == request.Username
                                         select users).FirstOrDefault();

                if (user == null)
                {
                    return "Usuario no encontrado";
                }
                else
                {
                    var alertaSeguimiento = new AlertaSeguimiento()
                    {
                        CreatedByUserId = user.Id,
                        DateCreated = new DateTime(),
                        EstadoId = request.EstadoId,
                        AlertaId = request.AlertaId,
                        Observaciones = request.Observaciones,
                        SeguimientoId = request.SeguimientoId,
                        UltimaFechaSeguimiento = new DateTime(),
                    };

                    _context.AlertaSeguimientos.Add(alertaSeguimiento);
                    _context.SaveChanges();

                    return "Alerta creada exitosamente";
                }
            }
            catch (Exception)
            {
                return "Se presento un problema al crear la alerta";
            }
        }

        public string GestionarAlerta(GestionarAlertaRequest request)
        {
            try
            {
                ApplicationUser? user = (from users in _context.Users
                                         where users.UserName == request.UserName
                                         select users).FirstOrDefault();

                if (user == null)
                {
                    return "Usuario no encontrado";
                }
                else
                {
                    AlertaSeguimiento? seguimiento = (from aSeguimiento in _context.AlertaSeguimientos
                                                      where aSeguimiento.Id == request.IdSeguimiento
                                                      select aSeguimiento).FirstOrDefault();

                    if (seguimiento == null)
                    {
                        AlertaSeguimiento alertaSeguimiento = new()
                        {
                            AlertaId = request.IdAlerta,
                            CreatedByUserId = user.Id,
                            DateCreated = new DateTime(),
                            EstadoId = request.IdEstado,
                            Observaciones = request.Observacion,
                            UltimaFechaSeguimiento = new DateTime()
                        };
                        _context.AlertaSeguimientos.Add(alertaSeguimiento);
                        _context.SaveChanges();

                        return "Seguimiento registrado exitosamente";
                    }
                    else
                    {
                        seguimiento.Observaciones = request.Observacion;
                        seguimiento.EstadoId = request.IdEstado;
                        seguimiento.DateUpdated = new DateTime();
                        seguimiento.UltimaFechaSeguimiento = new DateTime();
                        seguimiento.UpdatedByUserId = user.Id;

                        _context.Update(seguimiento);
                        _context.SaveChanges();

                        return "Seguimiento actualizado exitosamente";
                    }

                }
            }
            catch (Exception)
            {
                return "Se presento un problema al gestionar el seguimiento";
            }
        }

        public List<AlertaSeguimiento> ConsultarAlertaSeguimiento(ConsultarAlertasRequest request)
        {
            List<AlertaSeguimiento> response = (from aseg in _context.AlertaSeguimientos
                                                where aseg.SeguimientoId == request.IdSeguimiento
                                                select aseg).ToList();

            return response;
        }

        public async Task<AlertaSeguimientoDto[]> ConsultarAlertasUltimoSeguimiento(int idNNA)
        {
            var query = from s in _context.Seguimientos
                        join n in _context.NNAs on s.NNAId equals n.Id
                        where n.Id == idNNA
                        group s by s.NNAId into g
                        select new { id = g.Max(x => x.Id) };


            var response = await (from q in query
                                  join ase in _context.AlertaSeguimientos on q.id equals ase.SeguimientoId
                                  join a in _context.Alertas on ase.AlertaId equals a.Id
                                  join ea in _context.TPEstadoAlerta on ase.EstadoId equals ea.Id
                                  join sca in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                  where ea.Id == 1 || ea.Id == 3
                                  select new AlertaSeguimientoDto
                                  {
                                      Id = ea.Id,
                                      IdAlerta = sca.Id,
                                      Nombre = sca.CategoriaAlertaId + "." + sca.Indicador
                                  }).ToArrayAsync();

            return response;
        }

        public List<AlertaSeguimiento> ConsultarAlertaEstados(ConsultarAlertasEstadosRequest request)
        {
            List<AlertaSeguimiento> alertasSeguimiento = _context.AlertaSeguimientos
                              .Where(u => request.estados.Contains(u.EstadoId))
                              .ToList();

            return alertasSeguimiento;
        }
    }
}
