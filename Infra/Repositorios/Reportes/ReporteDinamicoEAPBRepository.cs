using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteDinamicoEAPBRepository : IReporteDinamicoEAPBRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly TablaParametricaService tablaParametricaService, _tablaParametricaService;

        public ReporteDinamicoEAPBRepository(ApplicationDbContext context, TablaParametricaService tablaParametricaService)
        {
            _context = context;
            _tablaParametricaService = tablaParametricaService;
        }

        public async Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            try
            {
                // Proyección de solo los campos necesarios de NNAs
                var nnas = await _context.NNAs
                    .Select(nna => new
                    {
                        nna.Id,
                        nna.EAPBId,
                        nna.TipoRegimenSSId
                    })
                    .ToListAsync(cancellationToken);

                var seguimientos = await _context.Seguimientos
                    .Where(s => s.FechaSeguimiento.HasValue &&
                                s.FechaSeguimiento >= fechaInicio &&
                                s.FechaSeguimiento <= fechaFin)
                    .ToListAsync(cancellationToken);

                var reporte = new List<ReporteDinamicoEAPBDTO>();

                // Agrupar por EAPBId
                var grupos = nnas
                    .GroupJoin(
                        seguimientos,
                        nna => nna.Id,
                        seg => seg.NNAId,
                        (nna, seguimientosAgrupados) => new { nna.EAPBId, nna.TipoRegimenSSId, Seguimientos = seguimientosAgrupados })
                    .GroupBy(g => g.EAPBId)  // Agrupa por EAPBId
                    .OrderBy(grupo => grupo.Key ?? 0)   // Ordenar por EAPBId
                    .ToList();

                foreach (var grupo in grupos)
                {
                    int? eapbIdInt = grupo.Key ?? 0;


                    var eapbName = string.Empty;
                    if(grupo.Key != null)
                    {
                        var list = await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", eapbIdInt, cancellationToken);
                        eapbName = (list is null) ? string.Empty : list.FirstOrDefault()?.Nombre ?? string.Empty;
                    }

                    reporte.Add(new ReporteDinamicoEAPBDTO
                    {
                        EAPBId = grupo.Key ?? 0,
                        EAPB = eapbName,
                        CasosAsociados = grupo.Sum(g => g.Seguimientos.Count()),
                        CasosConAlertasSinResolver = grupo.Sum(g => g.Seguimientos.Count(seg => _context.AlertaSeguimientos.Any(als => als.SeguimientoId == seg.NNAId && als.EstadoId == 3))),
                        TotalDeAlertasSinResolver = grupo.Sum(g => g.Seguimientos.Sum(seg => _context.AlertaSeguimientos.Count(als => als.SeguimientoId == seg.NNAId && als.EstadoId == 3))),
                        PromedioTiempoRespuestaAlertas = grupo
                            .SelectMany(g => g.Seguimientos)
                            .Where(seg => _context.AlertaSeguimientos.Any(als => als.SeguimientoId == seg.NNAId && als.UltimaFechaSeguimiento.HasValue))
                            .Select(seg => _context.AlertaSeguimientos
                                .Where(als => als.SeguimientoId == seg.NNAId && als.UltimaFechaSeguimiento.HasValue)
                                .Select(als => (seg.FechaSeguimiento.Value - als.UltimaFechaSeguimiento.Value).Days)
                                .FirstOrDefault())
                            .DefaultIfEmpty(0)
                            .Average(),
                        CasosRegimenContributivo = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "2")),
                        CasosRegimenSubsidiado = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "1")),
                        CasosRegimenEspecial = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "3")),
                        CasosRegimenExcepcion = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "4")),
                        CasosRegimenNoAfiliado = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "5")),
                        TotalAlertasResueltas = grupo.Sum(g => g.Seguimientos.Sum(seg => _context.AlertaSeguimientos.Count(als => als.SeguimientoId == seg.NNAId && als.EstadoId == 4))),
                        CasosSeguimientoPorIniciar = grupo.Sum(g => g.Seguimientos.Count(seg => seg.EstadoId == 1)),
                        CasosSeguimientoEnProceso = grupo.Sum(g => g.Seguimientos.Count(seg => seg.EstadoId == 2)),
                        CasosSeguimientoCulminado = grupo.Sum(g => g.Seguimientos.Count(seg => seg.EstadoId == 3))
                    });
                }

                return reporte;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al obtener el reporte", ex);
            }
        }

    }
}
