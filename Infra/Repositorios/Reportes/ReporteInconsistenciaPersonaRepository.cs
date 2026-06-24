using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos;
using Core.Modelos.TablasParametricas;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Infra.Repositorios.Reportes
{
    public class ReporteInconsistenciaPersonaRepository : IReporteInconsistenciaPersonaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly TablaParametricaService _tablaParametricaService;

        public ReporteInconsistenciaPersonaRepository(ApplicationDbContext context, TablaParametricaService tablaParametricaService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tablaParametricaService = tablaParametricaService;
        }

        public async Task<ReporteInconsistenciaPersona> AddReporteInconsistenciaAsync(ReporteInconsistenciaPersona reporte)
        {
            try
            {
                if (reporte == null)
                    throw new ArgumentNullException(nameof(reporte), "El reporte no puede ser nulo.");

                _context.ReporteInconsistenciaPersona.Add(reporte);
                await _context.SaveChangesAsync();

                return reporte;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error al agregar el reporte a la base de datos.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error inesperado al agregar el reporte.", ex);
            }
        }

        public async Task<ReporteInconsistenciaPersona> GetReporteInconsistenciaPersonaByIdAsync(long NNAId)
        {
            try
            {
                var reporte = await _context.ReporteInconsistenciaPersona.FirstOrDefaultAsync(r => r.NNAId == NNAId);

                if (reporte == null)
                    throw new KeyNotFoundException($"No se encontró ningún reporte con NNAId: {NNAId}.");

                return reporte;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ocurrió un error al obtener el reporte con NNAId: {NNAId}.", ex);
            }
        }

        public async Task<List<ReporteInconsistenciaPersona>> GetReporteInconsistenciasPersonaAsync()
        {
            try
            {
                return await _context.ReporteInconsistenciaPersona.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al obtener la lista de reportes de inconsistencias.", ex);
            }
        }

        public async Task<bool> MarcarResueltoAsync(long reporteId, string? userId)
        {
            var reporte = await _context.ReporteInconsistenciaPersona.FirstOrDefaultAsync(r => r.Id == reporteId);
            if (reporte == null) return false;
            reporte.Resuelto = true;
            reporte.FechaResolucion = DateTime.Now;
            reporte.ResueltoPorUserId = userId;
            reporte.ValidacionTipo = "Manual";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarReportesPreviosNNAAsync(long nnaId)
        {
            return await _context.ReporteInconsistenciaPersona.CountAsync(r => r.NNAId == nnaId);
        }

        public async Task<InconsistenciaReporte> GetReporteInconsistenciasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            // Periodo inclusivo
            var fin = fechaFin.Date.AddDays(1).AddTicks(-1);

            var reportes = await _context.ReporteInconsistenciaPersona
                .Where(r => r.FechaReporte >= fechaInicio && r.FechaReporte <= fin)
                .ToListAsync();

            var nnaIds = reportes.Select(r => r.NNAId).Distinct().ToList();

            var nnasEnPeriodo = await _context.NNAs
                .Where(n => nnaIds.Contains(n.Id))
                .ToListAsync();

            // ---------- KPI 2: por tipo de campo (22 campos HU) ----------
            var camposComparados = new Dictionary<string, int>
            {
                { "TipoIdentificacion",  reportes.Count(r => Norm(r.TipoIdentificacion) != Norm(r.TipoIdentificacionSIVIGILA)) },
                { "NumeroIdentificacion", reportes.Count(r => Norm(r.NumeroIdentificacion) != Norm(r.NumeroIdentificacionSIVIGILA)) },
                { "PrimerNombre",         reportes.Count(r => Norm(r.PrimerNombre) != Norm(r.PrimerNombreSIVIGILA)) },
                { "SegundoNombre",        reportes.Count(r => Norm(r.SegundoNombre) != Norm(r.SegundoNombreSIVIGILA)) },
                { "PrimerApellido",       reportes.Count(r => Norm(r.PrimerApellido) != Norm(r.PrimerApellidoSIVIGILA)) },
                { "SegundoApellido",      reportes.Count(r => Norm(r.SegundoApellido) != Norm(r.SegundoApellidoSIVIGILA)) },
                { "FechaNacimiento",      reportes.Count(r => r.FechaNacimiento != r.FechaNacimientoSIVIGILA) },
                { "Sexo",                 reportes.Count(r => Norm(r.SexoId) != Norm(r.SexoIdSIVIGILA)) },
                { "FechaDefuncion",       reportes.Count(r => r.FechaDefuncion != r.FechaDefuncionSIVIGILA) },
            };

            // Campos sin pareja SIVIGILA — contar NNAs con campo NULL/empty en período
            var nnasPeriodoFull = await _context.NNAs
                .Where(n => n.DateCreated >= fechaInicio && n.DateCreated <= fin && n.DateDeleted == null)
                .ToListAsync();

            int countNull(Func<NNAs, bool> pred) => nnasPeriodoFull.Count(pred);

            var camposFalta = new Dictionary<string, int>
            {
                { "Diagnostico",         countNull(n => string.IsNullOrEmpty(n.TipoCancerId)) },
                { "Estado",              countNull(n => n.estadoId == null) },
                { "FechaNotificacion",   countNull(n => n.FechaNotificacionSIVIGILA == null || n.FechaNotificacionSIVIGILA == DateTime.MinValue) },
                { "PaisNacimiento",      countNull(n => string.IsNullOrEmpty(n.DepartamentoNacimientoId)) },
                { "RegimenAfiliacion",   countNull(n => string.IsNullOrEmpty(n.TipoRegimenSSId)) },
                { "Asegurador",          countNull(n => n.EAPBId == null) },
                { "IPS",                 countNull(n => n.IPSId == null) },
                { "Contacto1Nombre",     countNull(n => string.IsNullOrEmpty(n.CuidadorNombres)) },
                { "Contacto1Parentesco", countNull(n => n.CuidadorParentescoId == null) },
                { "Contacto1Telefono",   countNull(n => string.IsNullOrEmpty(n.CuidadorTelefono)) },
            };

            var inconsistenciasPorCampo = camposComparados.Concat(camposFalta).ToDictionary(k => k.Key, v => v.Value);
            int totalInconsistencias = inconsistenciasPorCampo.Values.Sum();

            // ---------- KPI 3: por fuente datos ----------
            var fuentesAgrupadas = reportes.GroupBy(r => r.FuenteDatos ?? "SIVIGILA")
                                           .Select(g => new { Fuente = g.Key, Total = g.Count() })
                                           .ToList();
            var totalFuentes = fuentesAgrupadas.Sum(f => f.Total);
            var fuentesEsperadas = new[] { "SIVIGILA", "RUAF", "BDUA", "MIPRES" };
            var inconsistenciasPorFuente = fuentesEsperadas.Select(f =>
            {
                var match = fuentesAgrupadas.FirstOrDefault(x => x.Fuente == f);
                int total = match?.Total ?? 0;
                return new InconsistenciaFuente
                {
                    Fuente = f,
                    TotalInconsistencias = total,
                    Porcentaje = totalFuentes > 0 ? Math.Round((double)total / totalFuentes * 100, 2) : 0
                };
            }).ToList();

            // ---------- KPI 4: por departamento + municipio ----------
            var deptoGroups = nnasEnPeriodo
                .Where(n => !string.IsNullOrEmpty(n.DepartamentoTratamientoId) ||
                            (!string.IsNullOrEmpty(n.ResidenciaActualMunicipioId) && n.ResidenciaActualMunicipioId.Length >= 2))
                .GroupBy(n => n.DepartamentoTratamientoId ?? n.ResidenciaActualMunicipioId.Substring(0, 2))
                .Select(g => new { DeptoId = g.Key, Total = g.Count() })
                .ToList();

            var inconsistenciasPorDepartamento = new List<InconsistenciaDepartamento>();
            foreach (var d in deptoGroups)
            {
                string nombre = string.Empty;
                if (!string.IsNullOrEmpty(d.DeptoId) && int.TryParse(d.DeptoId, out int codDepto))
                {
                    var lookup = await _tablaParametricaService.GetBynomTREFCodigo("Departamento", codDepto, default);
                    nombre = lookup?.FirstOrDefault()?.Nombre ?? string.Empty;
                }
                inconsistenciasPorDepartamento.Add(new InconsistenciaDepartamento
                {
                    DepartamentoId = d.DeptoId,
                    Departamento = nombre,
                    TotalInconsistencias = d.Total,
                    Porcentaje = nnasEnPeriodo.Count > 0 ? Math.Round((double)d.Total / nnasEnPeriodo.Count * 100, 2) : 0
                });
            }

            var municGroups = nnasEnPeriodo
                .Where(n => !string.IsNullOrEmpty(n.ResidenciaActualMunicipioId))
                .GroupBy(n => n.ResidenciaActualMunicipioId)
                .Select(g => new { MunicId = g.Key, Total = g.Count() })
                .ToList();

            var inconsistenciasPorMunicipio = new List<InconsistenciaMunicipio>();
            foreach (var m in municGroups)
            {
                string nombre = string.Empty, depto = string.Empty;
                if (!string.IsNullOrEmpty(m.MunicId) && int.TryParse(m.MunicId, out int codMun))
                {
                    var lookup = await _tablaParametricaService.GetBynomTREFCodigo("Municipio", codMun, default);
                    nombre = lookup?.FirstOrDefault()?.Nombre ?? string.Empty;
                }
                if (m.MunicId?.Length >= 2 && int.TryParse(m.MunicId.Substring(0, 2), out int codDep))
                {
                    var lookupD = await _tablaParametricaService.GetBynomTREFCodigo("Departamento", codDep, default);
                    depto = lookupD?.FirstOrDefault()?.Nombre ?? string.Empty;
                }
                inconsistenciasPorMunicipio.Add(new InconsistenciaMunicipio
                {
                    MunicipioId = m.MunicId,
                    Municipio = nombre,
                    Departamento = depto,
                    TotalInconsistencias = m.Total,
                    Porcentaje = nnasEnPeriodo.Count > 0 ? Math.Round((double)m.Total / nnasEnPeriodo.Count * 100, 2) : 0
                });
            }

            // ---------- KPI 5: tipo cáncer + diagnóstico CIE10 ----------
            var tipoCancerGroups = nnasEnPeriodo
                .Where(n => !string.IsNullOrEmpty(n.TipoCancerId))
                .GroupBy(n => n.TipoCancerId)
                .Select(g => new { TipoId = g.Key, Total = g.Count() })
                .ToList();

            var inconsistenciasPorTipoCancer = new List<InconsistenciaTipoCancer>();
            foreach (var t in tipoCancerGroups)
            {
                string nombre = string.Empty;
                if (!string.IsNullOrEmpty(t.TipoId) && int.TryParse(t.TipoId, out int codTC))
                {
                    var lookup = await _tablaParametricaService.GetBynomTREFCodigo("TipoCancer", codTC, default);
                    nombre = lookup?.FirstOrDefault()?.Nombre ?? string.Empty;
                }
                inconsistenciasPorTipoCancer.Add(new InconsistenciaTipoCancer
                {
                    TipoCancerId = t.TipoId,
                    TipoCancer = nombre,
                    TotalInconsistencias = t.Total,
                    Porcentaje = nnasEnPeriodo.Count > 0 ? Math.Round((double)t.Total / nnasEnPeriodo.Count * 100, 2) : 0
                });
            }

            var cie10List = await _context.CIE10s.ToListAsync();
            var inconsistenciasPorDiagnostico = nnasEnPeriodo
                .Where(n => n.TipoDiagnosticoId != null)
                .GroupBy(n => n.TipoDiagnosticoId)
                .Select(g =>
                {
                    int.TryParse(g.Key, out int parsedKey);
                    var cie = cie10List.FirstOrDefault(c => c.Id == parsedKey);
                    return new InconsistenciaDiagnostico
                    {
                        DiagnosticoId = parsedKey,
                        Diagnostico = cie?.Nombre ?? "(Sin diagnóstico)",
                        TotalInconsistencias = g.Count(),
                        Porcentaje = nnasEnPeriodo.Count > 0 ? Math.Round((double)g.Count() / nnasEnPeriodo.Count * 100, 2) : 0
                    };
                })
                .ToList();

            // ---------- KPI 6: tiempo promedio resolución ----------
            var resueltos = reportes.Where(r => r.Resuelto && r.FechaResolucion.HasValue).ToList();
            double tiempoPromedioDias = resueltos.Count > 0
                ? Math.Round(resueltos.Average(r => (r.FechaResolucion!.Value - r.FechaReporte).TotalDays), 2)
                : 0;
            int totalResueltos = resueltos.Count;
            int totalPendientes = reportes.Count - totalResueltos;

            // ---------- KPI 7: auto vs manual ----------
            int validadasAuto = reportes.Count(r => (r.ValidacionTipo ?? "Automatica") == "Automatica");
            int validadasManual = reportes.Count(r => r.ValidacionTipo == "Manual");

            // ---------- KPI 8: reincidencia ----------
            int totalReincidentes = reportes.Count(r => r.EsReincidente);
            double tasaReincidencia = reportes.Count > 0
                ? Math.Round((double)totalReincidentes / reportes.Count * 100, 2)
                : 0;

            // ---------- KPI 9: impacto tiempo notif / tratamiento ----------
            var nnasConIncons = nnasEnPeriodo
                .Where(n => n.FechaNotificacionSIVIGILA.HasValue && n.FechaNotificacionSIVIGILA.Value != DateTime.MinValue
                            && n.FechaInicioTratamiento.HasValue)
                .ToList();
            var allNnasPeriodoConTratamiento = nnasPeriodoFull
                .Where(n => n.FechaNotificacionSIVIGILA.HasValue && n.FechaNotificacionSIVIGILA.Value != DateTime.MinValue
                            && n.FechaInicioTratamiento.HasValue)
                .ToList();
            var nnasSinIncons = allNnasPeriodoConTratamiento.Where(n => !nnaIds.Contains(n.Id)).ToList();

            double avgNotifConIncons = nnasConIncons.Count > 0
                ? nnasConIncons.Average(n => (n.FechaInicioTratamiento!.Value - n.FechaNotificacionSIVIGILA!.Value).TotalDays)
                : 0;
            double avgNotifSinIncons = nnasSinIncons.Count > 0
                ? nnasSinIncons.Average(n => (n.FechaInicioTratamiento!.Value - n.FechaNotificacionSIVIGILA!.Value).TotalDays)
                : 0;
            double impactoNotif = Math.Round(avgNotifConIncons - avgNotifSinIncons, 2);

            var nnasConInconsDiag = nnasEnPeriodo.Where(n => n.FechaDiagnostico.HasValue && n.FechaInicioTratamiento.HasValue).ToList();
            var allNnasPeriodoDiag = nnasPeriodoFull.Where(n => n.FechaDiagnostico.HasValue && n.FechaInicioTratamiento.HasValue).ToList();
            var nnasSinInconsDiag = allNnasPeriodoDiag.Where(n => !nnaIds.Contains(n.Id)).ToList();

            double avgTratConIncons = nnasConInconsDiag.Count > 0
                ? nnasConInconsDiag.Average(n => (n.FechaInicioTratamiento!.Value - n.FechaDiagnostico!.Value).TotalDays)
                : 0;
            double avgTratSinIncons = nnasSinInconsDiag.Count > 0
                ? nnasSinInconsDiag.Average(n => (n.FechaInicioTratamiento!.Value - n.FechaDiagnostico!.Value).TotalDays)
                : 0;
            double impactoTrat = Math.Round(avgTratConIncons - avgTratSinIncons, 2);

            // ---------- KPI 10: campos críticos trazabilidad ----------
            int totalNnas = nnasPeriodoFull.Count;
            CampoCriticoTrazabilidad mk(string label, Func<NNAs, bool> faltaPred)
            {
                int falt = nnasPeriodoFull.Count(faltaPred);
                return new CampoCriticoTrazabilidad
                {
                    Campo = label,
                    TotalNNAs = totalNnas,
                    NNAsConFalta = falt,
                    PorcentajeInconsistencia = totalNnas > 0 ? Math.Round((double)falt / totalNnas * 100, 2) : 0
                };
            }
            var camposCriticos = new List<CampoCriticoTrazabilidad>
            {
                mk("Fecha de inicio de tratamiento", n => !n.FechaInicioTratamiento.HasValue),
                mk("Fecha de diagnóstico", n => !n.FechaDiagnostico.HasValue),
                mk("Fecha consulta diagnóstico", n => !n.FechaConsultaDiagnostico.HasValue),
                mk("Fecha inicio síntomas", n => !n.FechaInicioSintomas.HasValue),
                mk("Fecha hospitalización", n => !n.FechaHospitalizacion.HasValue),
            };

            return new InconsistenciaReporte
            {
                TotalInconsistencias = totalInconsistencias,
                InconsistenciasPorCampo = inconsistenciasPorCampo,
                InconsistenciasPorFuente = inconsistenciasPorFuente,
                InconsistenciasPorDepartamento = inconsistenciasPorDepartamento,
                InconsistenciasPorMunicipio = inconsistenciasPorMunicipio,
                InconsistenciasPorTipoCancer = inconsistenciasPorTipoCancer,
                InconsistenciasPorDiagnostico = inconsistenciasPorDiagnostico,
                TiempoPromedioResolucionDias = tiempoPromedioDias,
                TotalResueltos = totalResueltos,
                TotalPendientes = totalPendientes,
                ValidadasAutomaticamente = validadasAuto,
                ValidadasManualmente = validadasManual,
                TasaReincidencia = tasaReincidencia,
                TotalReincidentes = totalReincidentes,
                ImpactoNotificacionDias = impactoNotif,
                ImpactoTratamientoDias = impactoTrat,
                CamposCriticosTrazabilidad = camposCriticos
            };
        }

        private static string Norm(string? s) => (s ?? string.Empty).Trim().ToUpper();
    }
}
