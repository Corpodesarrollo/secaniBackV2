using Core.DTOs;

namespace Infra.Repositorios
{
    public class SeguimientoRepo(ApplicationDbContext db)
    {

        public IQueryable<SeguimientoDto> GetSelect(string id)
        {
            return from s in db.Seguimientos
                   join e in db.TPEstadoNNA on s.EstadoId equals e.Id
                   join n in db.NNAs on s.NNAId equals n.Id
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
                       Alertas = (from als in db.AlertaSeguimientos
                                  join a in db.Alertas on als.AlertaId equals a.Id
                                  join ea in db.TPEstadoAlerta on als.EstadoId equals ea.Id
                                  join sca in db.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                  select new AlertaSeguimientoDto { Nombre = sca.CategoriaAlertaId + "." + sca.Indicador, Id = ea.Id }).ToList()
                   };
        }

        public List<SeguimientoDto> GetAllByIdUser(string id, int filtro)
        {
            try
            {
                var query = GetSelect(id);
                var result = query.ToList();

                if (filtro == 1) //hoy
                {
                    return result.Where(x => x.FechaNotificacion.Date == DateTime.Now.Date).ToList();
                }
                else if (filtro == 2) //con alerta
                {
                    return result.Where(x => x.Alertas.Count > 0).ToList();
                }
                else if (filtro == 3) //Todos
                {
                    return result;
                }
                else if (filtro == 4) //Solicitados por Cuidador
                {
                    return result.Where(x => x.AsuntoUltimaActuacion.ToLower() == "solicitado por cuidador").ToList();
                }


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public SeguimientoCntFiltrosDto SeguimientoCntFiltros(string id)
        {
            try
            {
                var query = GetSelect(id);
                var result = query.ToList();

                return new SeguimientoCntFiltrosDto
                {
                    Todos = result.Count,
                    Hoy = result.Where(x => x.FechaNotificacion.Date == DateTime.Now.Date).Count(),
                    ConAlerta = result.Where(x => x.Alertas.Count > 0).Count(),
                    SolicitadosPorCuidador = result.Where(x => x.AsuntoUltimaActuacion.ToLower() == "solicitado por cuidador").Count()
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
