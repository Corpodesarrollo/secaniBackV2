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

        public async Task<List<ReporteDinamicoEAPBDTO>> GetReporteDinamicoEAPBAsync(DateTime fechaInicio, DateTime fechaFin, int? eapbId, string? departamentoId, CancellationToken cancellationToken)
        {
            try
            {
                // Proyección de solo los campos necesarios de NNAs (filtros opcionales)
                var query = _context.NNAs.AsQueryable();
                if (eapbId.HasValue && eapbId.Value > 0)
                    query = query.Where(n => n.EAPBId == eapbId.Value);
                if (!string.IsNullOrWhiteSpace(departamentoId))
                    query = query.Where(n => n.DepartamentoTratamientoId == departamentoId
                                          || (n.ResidenciaActualMunicipioId != null
                                              && n.ResidenciaActualMunicipioId.Length >= 2
                                              && n.ResidenciaActualMunicipioId.Substring(0, 2) == departamentoId));

                var nnas = await query
                    .Select(nna => new
                    {
                        nna.Id,
                        nna.EAPBId,
                        nna.TipoRegimenSSId
                    })
                    .ToListAsync(cancellationToken);

                // Periodo inclusivo
                var fin = fechaFin.Date.AddDays(1).AddTicks(-1);
                var seguimientos = await _context.Seguimientos
                    .Where(s => s.FechaSeguimiento.HasValue &&
                                s.FechaSeguimiento >= fechaInicio &&
                                s.FechaSeguimiento <= fin)
                    .ToListAsync(cancellationToken);

                // Alinea con /gestionar-alertas (que NO filtra fecha): traer ultimo snapshot
                // GLOBAL por AlertaId + mapearlo al NNA (via seguimiento) para agrupar EAPB.
                var nnaIdsTodos = nnas.Select(n => n.Id).ToList();
                // Indice Seguimiento -> NNAId (todos los seguimientos, no solo periodo)
                var todoSeguimientos = await _context.Seguimientos
                    .Select(s => new { s.Id, s.NNAId })
                    .ToListAsync(cancellationToken);
                var nnaPorSegId = todoSeguimientos.ToDictionary(s => s.Id, s => s.NNAId);
                // Ultimo snapshot por AlertaId: GroupBy+Max -> IN -> re-load
                var ultimosIds = await _context.AlertaSeguimientos
                    .GroupBy(a => a.AlertaId)
                    .Select(g => g.Max(x => x.Id))
                    .ToListAsync(cancellationToken);
                var ultimosAlertas = await _context.AlertaSeguimientos
                    .Where(a => ultimosIds.Contains(a.Id))
                    .Select(a => new { a.Id, a.SeguimientoId, a.AlertaId, a.EstadoId, a.UltimaFechaSeguimiento })
                    .ToListAsync(cancellationToken);
                // Mapear a NNAId
                var ultimasConNNA = ultimosAlertas
                    .Where(a => nnaPorSegId.ContainsKey(a.SeguimientoId))
                    .Select(a => new { a.AlertaId, a.EstadoId, a.UltimaFechaSeguimiento, NNAId = nnaPorSegId[a.SeguimientoId] })
                    .ToList();
                // Indice por NNAId
                var alertasUltPorNNA = ultimasConNNA.GroupBy(a => a.NNAId).ToDictionary(g => g.Key, g => g.ToList());

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
                    if (grupo.Key != null)
                    {
                        //busca por EAPB de sispro
                        var list = await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", eapbIdInt, cancellationToken);
                        eapbName = (list is null) ? string.Empty : list.FirstOrDefault()?.Nombre ?? string.Empty;
                        //si no encuentra busca en la tabla de eapb local
                        if (eapbName == string.Empty)
                        {
                            var eapb = await _context.TPEAPB.FirstOrDefaultAsync(x => x.Id == eapbIdInt);
                            if (eapb != null)
                                eapbName = eapb?.Nombre ?? string.Empty;
                            else
                            {
                                eapb = (await _context.TPEAPB.ToListAsync()).FirstOrDefault(x => int.TryParse(x.Codigo, out var c) && c == eapbIdInt);
                                if (eapb != null)
                                    eapbName = eapb?.Nombre ?? string.Empty;
                            }
                        }
                    }

                    reporte.Add(new ReporteDinamicoEAPBDTO
                    {
                        EAPBId = grupo.Key ?? 0,
                        EAPB = eapbName,
                        // Alerts agregadas por NNAs del EAPB usando ultimo snapshot GLOBAL.
                        CasosAsociados = grupo.Sum(g => g.Seguimientos.Count()),
                        CasosConAlertasSinResolver = grupo
                            .Where(g => alertasUltPorNNA.TryGetValue(g.Seguimientos.FirstOrDefault()?.NNAId ?? -1, out var lista) &&
                                        lista.Any(a => a.EstadoId == 1 || a.EstadoId == 2 || a.EstadoId == 3))
                            .Select(g => g.Seguimientos.FirstOrDefault()?.NNAId ?? 0)
                            .Distinct().Count(),
                        TotalDeAlertasSinResolver = grupo
                            .SelectMany(g => alertasUltPorNNA.TryGetValue(g.Seguimientos.FirstOrDefault()?.NNAId ?? -1, out var lista)
                                ? lista.Where(a => a.EstadoId == 1 || a.EstadoId == 2 || a.EstadoId == 3)
                                : Enumerable.Empty<dynamic>())
                            .Select(a => (long)a.AlertaId)
                            .Distinct().Count(),
                        // Orden invertido: UltimaFechaSeguimiento (cuando se actuo sobre alerta)
                        // - FechaSeguimiento (cuando se registro el seguimiento). Abs como
                        // safeguard contra fechas mal ordenadas en BD.
                        PromedioTiempoRespuestaAlertas = grupo
                            .SelectMany(g => g.Seguimientos)
                            .Select(seg =>
                            {
                                var als = ultimosAlertas.FirstOrDefault(a => a.SeguimientoId == seg.Id && a.UltimaFechaSeguimiento.HasValue);
                                if (als != null && seg.FechaSeguimiento.HasValue)
                                    return Math.Abs((als.UltimaFechaSeguimiento!.Value - seg.FechaSeguimiento.Value).Days);
                                return 0;
                            })
                            .DefaultIfEmpty(0)
                            .Average(),
                        CasosRegimenContributivo = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "2")),
                        CasosRegimenSubsidiado = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "1")),
                        CasosRegimenEspecial = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "3")),
                        CasosRegimenExcepcion = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "4")),
                        CasosRegimenNoAfiliado = grupo.Sum(g => g.Seguimientos.Count(seg => g.TipoRegimenSSId == "5")),
                        TotalAlertasResueltas = grupo
                            .SelectMany(g => alertasUltPorNNA.TryGetValue(g.Seguimientos.FirstOrDefault()?.NNAId ?? -1, out var lista)
                                ? lista.Where(a => a.EstadoId == 4)
                                : Enumerable.Empty<dynamic>())
                            .Select(a => (long)a.AlertaId)
                            .Distinct().Count(),
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
