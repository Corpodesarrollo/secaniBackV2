using Core.DTOs;
using Core.Interfaces;
using Core.Modelos;
using Core.Modelos.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class GestionarAlertasRepo(ApplicationDbContext db) : IGestionarAlertas
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
                               select new GestionarAlertasDto
                               {
                                   IdAlerta = ea.Id,
                                   IdAlertaSeguimiento = als.Id,
                                   Alerta = sca.CategoriaAlertaId + "." + sca.Indicador,
                                   NombreNNA = $"{n.PrimerNombre ?? ""} {n.SegundoNombre ?? ""} {n.PrimerApellido ?? ""} {n.SegundoApellido ?? ""}",
                                   Categoria = ca.Nombre,
                                   Subcategoria = sca.SubCategoriaAlerta,
                                   FechaNotificacion = s.FechaSeguimiento,
                                   Estado = ea.Nombre
                               }).ToList();

            alertasBase.ForEach(item =>
            {
                item.TextoEstado = item.IdAlerta switch
                {
                    4 => "RESUELTA",
                    1 or 2 or 3 or 5 => "SIN RESOLVER",
                    _ => "CERRADA"
                };

                item.ColorEstado = item.IdAlerta switch
                {
                    4 => "success",
                    1 or 2 => "warning",
                    3 or 5 => "danger",
                    _ => "secondary"
                };
            });

            return alertasBase;
        }

        public async Task<NotificacionEntidadDto> GetNotificacionEntidad(int idAlerta)
        {
            var result = await db.NotificacionesEntidad.FirstOrDefaultAsync(x => x.AlertaSeguimientoId == idAlerta) ?? throw new Exception("Notificación no encontrada");
            var data = GenericMapper.Map<NotificacionEntidad, NotificacionEntidadDto>(result);
            return data;
        }
    }
}
