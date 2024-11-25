using Core.Modelos;

namespace Core.Interfaces.Repositorios.Reportes
{
    public interface IReporteDepuracionRepository
    {
        Task<List<ReporteDepuracion>> GetReporteDepuraciones(DateTime FechaInicio, DateTime FechaFin);
    }
}
