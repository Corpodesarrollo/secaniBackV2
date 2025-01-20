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
                // Manejo de errores relacionados con la base de datos
                throw new InvalidOperationException("Error al agregar el reporte a la base de datos.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
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

        public async Task<InconsistenciaReporte> GetReporteInconsistenciasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var reportesFiltrados = _context.Set<ReporteInconsistenciaPersona>()
                .Where(r => r.FechaReporte >= fechaInicio && r.FechaReporte <= fechaFin);

            var totalTipoIdentificacion = await reportesFiltrados.CountAsync(r => r.TipoIdentificacion != r.TipoIdentificacionSIVIGILA);

            var totalNombres = await reportesFiltrados.CountAsync(r =>
                r.PrimerNombre.ToUpper() != r.PrimerNombreSIVIGILA.ToUpper() ||
                r.SegundoNombre.ToUpper() != r.SegundoNombreSIVIGILA.ToUpper() ||
                r.PrimerApellido.ToUpper() != r.PrimerApellidoSIVIGILA.ToUpper() ||
                r.SegundoApellido.ToUpper() != r.SegundoApellidoSIVIGILA.ToUpper()
            );

            var totalSexoId = await reportesFiltrados.CountAsync(r => r.SexoId != r.SexoIdSIVIGILA);

            var totalFechaDefuncion = await reportesFiltrados.CountAsync(r => r.FechaDefuncion != r.FechaDefuncionSIVIGILA);

            var totalInconsistencias = totalTipoIdentificacion + totalNombres + totalSexoId + totalFechaDefuncion;

            var inconsistenciasPorDepartamento = (await reportesFiltrados
                .Join(_context.Set<NNAs>(), r => r.NNAId, n => n.Id, (r, n) => new
                {
                    DepartamentoId = n.DepartamentoTratamientoId ?? n.ResidenciaActualMunicipioId.Substring(0, 2),
                    Inconsistencia = (r.TipoIdentificacion != r.TipoIdentificacionSIVIGILA ? 1 : 0) +
                                    (r.PrimerNombre.ToUpper() != r.PrimerNombreSIVIGILA.ToUpper() ? 1 : 0) +
                                    (r.SegundoNombre.ToUpper() != r.SegundoNombreSIVIGILA.ToUpper() ? 1 : 0) +
                                    (r.PrimerApellido.ToUpper() != r.PrimerApellidoSIVIGILA.ToUpper() ? 1 : 0) +
                                    (r.SegundoApellido.ToUpper() != r.SegundoApellidoSIVIGILA.ToUpper() ? 1 : 0) +
                                    (r.SexoId != r.SexoIdSIVIGILA ? 1 : 0) +
                                    (r.FechaDefuncion != r.FechaDefuncionSIVIGILA ? 1 : 0)
                })
                .ToListAsync())
                .GroupBy(x => x.DepartamentoId)
                .Select(async g => new InconsistenciaDepartamento
                {
                    DepartamentoId = g.Key,
                    Departamento = !string.IsNullOrEmpty(g.Key) && g.Key.Length >= 2 && int.TryParse(g.Key, out int codigoDepartamento)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo("Departamento", codigoDepartamento, default))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    TotalInconsistencias = g.Sum(x => x.Inconsistencia),
                    Porcentaje = (double)g.Sum(x => x.Inconsistencia) / totalInconsistencias * 100
                });

            var inconsistenciasPorDiagnostico = (await reportesFiltrados
                .Join(_context.Set<NNAs>(), r => r.NNAId, n => n.Id, (r, n) => new {
                    n.DiagnosticoId,
                    Inconsistencia =
                    (r.TipoIdentificacion != r.TipoIdentificacionSIVIGILA ? 1 : 0) +
                    (r.PrimerNombre.ToUpper() != r.PrimerNombreSIVIGILA.ToUpper() ? 1 : 0) +
                    (r.SegundoNombre.ToUpper() != r.SegundoNombreSIVIGILA.ToUpper() ? 1 : 0) +
                    (r.PrimerApellido.ToUpper() != r.PrimerApellidoSIVIGILA.ToUpper() ? 1 : 0) +
                    (r.SegundoApellido.ToUpper() != r.SegundoApellidoSIVIGILA.ToUpper() ? 1 : 0) +
                    (r.SexoId != r.SexoIdSIVIGILA ? 1 : 0) +
                    (r.FechaDefuncion != r.FechaDefuncionSIVIGILA ? 1 : 0)
                })
                .Join(_context.Set<TPCIE10>(), n => n.DiagnosticoId, d => d.Id, (n, d) => new { n.DiagnosticoId, Diagnostico = d.Nombre, n.Inconsistencia })
                .ToListAsync())
                .GroupBy(x => x.DiagnosticoId)
                .Select(g => new InconsistenciaDiagnostico
                {
                    DiagnosticoId = g.Key,
                    Diagnostico = g.First().Diagnostico,
                    TotalInconsistencias = g.Sum(x => x.Inconsistencia),
                    Porcentaje = (double)g.Sum(x => x.Inconsistencia) / totalInconsistencias * 100
                })
                .ToList();

            return new InconsistenciaReporte
            {
                TotalInconsistencias = totalInconsistencias,
                InconsistenciasPorCampo = new Dictionary<string, int>
            {
                { "TipoIdentificacion", totalTipoIdentificacion },
                { "Nombres", totalNombres },
                { "SexoId", totalSexoId },
                { "FechaDefuncion", totalFechaDefuncion }
            },
                InconsistenciasPorDepartamento = (await Task.WhenAll(inconsistenciasPorDepartamento)).ToList(),
                InconsistenciasPorDiagnostico = inconsistenciasPorDiagnostico
            };
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
    }

}
