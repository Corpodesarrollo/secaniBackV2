using Core.DTOs.MSTablasParametricas;
using Core.DTOs.Reportes;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos.TablasParametricas;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteDinamicoNNARepository(
        ApplicationDbContext context,
        IGenericService<TPCIE10, CIE10DTO> diagnosticoService,
        IGenericService<TPOrigenReporte, GenericTPDTO> origenReporteService,
        TablaParametricaService tablaParametricaService,
        IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> estadoIngresoEstrategiaService
        ) : IReporteDinamicoNNARepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IGenericService<TPCIE10, CIE10DTO> _diagnosticoService = diagnosticoService;
        private readonly IGenericService<TPOrigenReporte, GenericTPDTO> _origenReporteService = origenReporteService;
        private readonly TablaParametricaService _tablaParametricaService = tablaParametricaService;
        private readonly IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> _estadoIngresoEstrategiaService = estadoIngresoEstrategiaService;

        public async Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            var nnas = await _context.NNAs
                .Where(nna => nna.FechaIngresoEstrategia >= fechaInicio && nna.FechaIngresoEstrategia <= fechaFin)
                .ToListAsync(cancellationToken);

            var reporte = new List<ReporteDinamicoNNADTO>();

            foreach (var nna in nnas)
            {
                var dto = new ReporteDinamicoNNADTO
                {
                    Id = nna.Id,
                    PrimerNombre = nna.PrimerNombre,
                    SegundoNombre = nna.SegundoNombre,
                    PrimerApellido = nna.PrimerApellido,
                    SegundoApellido = nna.SegundoApellido,
                    FechaNacimiento = nna.FechaNacimiento,
                    Edad = nna.FechaNacimiento.HasValue
                        ? (int)((DateTime.Now - nna.FechaNacimiento.Value).TotalDays / 365.25)
                        : 0,
                    SexoId = nna.SexoId,
                    TipoIdentificacionId = nna.TipoIdentificacionId,
                    NumeroIdentificacion = nna.NumeroIdentificacion,
                    FechaNotificacionSIVIGILA = nna.FechaNotificacionSIVIGILA,
                    DiagnosticoId = nna.DiagnosticoId,
                    Diagnostico = nna.DiagnosticoId.HasValue
                        ? (await _diagnosticoService.GetByIdAsync(nna.DiagnosticoId.Value, cancellationToken))?.Nombre ?? nna.DiagnosticoId.ToString()
                        : string.Empty,
                    OrigenReporteId = nna.OrigenReporteId,
                    OrigenReporte = nna.OrigenReporteId.HasValue
                        ? (await _origenReporteService.GetByIdAsync(nna.OrigenReporteId.Value, cancellationToken))?.Nombre ?? nna.OrigenReporteId.ToString()
                        : string.Empty,
                    PaisId = nna.PaisId,
                    EtniaId = nna.EtniaId,
                    MunicipioNacimientoId = nna.MunicipioNacimientoId,
                    DepartamentoNacimiento = !string.IsNullOrEmpty(nna.MunicipioNacimientoId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Departamento",
                            int.Parse(nna.MunicipioNacimientoId.Substring(0, 2)),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    MunicipioNacimiento = !string.IsNullOrEmpty(nna.MunicipioNacimientoId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Municipio",
                            int.Parse(nna.MunicipioNacimientoId),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    GrupoPoblacionId = nna.GrupoPoblacionId,
                    ResidenciaOrigenMunicipioId = nna.ResidenciaOrigenMunicipioId,
                    DepartamentoProcedencia = !string.IsNullOrEmpty(nna.ResidenciaOrigenMunicipioId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Departamento",
                            int.Parse(nna.ResidenciaOrigenMunicipioId.Substring(0, 2)),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    MunicipioProcedencia = !string.IsNullOrEmpty(nna.ResidenciaOrigenMunicipioId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Municipio",
                            int.Parse(nna.ResidenciaOrigenMunicipioId),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    ResidenciaOrigenBarrio = nna.ResidenciaOrigenBarrio,
                    ResidenciaOrigenAreaId = nna.ResidenciaOrigenAreaId,
                    AreaProcedencia = !string.IsNullOrEmpty(nna.ResidenciaOrigenAreaId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo("ZonaTerritorial", int.Parse(nna.ResidenciaOrigenAreaId), cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    ResidenciaOrigenDireccion = nna.ResidenciaOrigenDireccion,
                    ResidenciaOrigenEstratoId = nna.ResidenciaOrigenEstratoId,
                    ResidenciaActualTelefono = nna.ResidenciaActualTelefono,
                    ResidenciaActualMunicipioId = nna.ResidenciaActualMunicipioId,
                    DepartamentoResidenciaActual = !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Departamento",
                            int.Parse(nna.ResidenciaActualMunicipioId.Substring(0, 2)),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    MunicipioResidenciaActual = !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId)
                        ? (await _tablaParametricaService.GetBynomTREFCodigo(
                            "Municipio",
                            int.Parse(nna.ResidenciaActualMunicipioId),
                            cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    EstadoIngresoEstrategiaId = nna.EstadoIngresoEstrategiaId,
                    EstadoIngresoEstrategia = nna.EstadoIngresoEstrategiaId.HasValue
                        ? (await _estadoIngresoEstrategiaService.GetByIdAsync(nna.EstadoIngresoEstrategiaId.Value, cancellationToken))?.Nombre ?? nna.EstadoIngresoEstrategiaId.ToString()
                        : string.Empty,
                    FechaIngresoEstrategia = nna.FechaIngresoEstrategia,
                    TipoRegimenSSId = nna.TipoRegimenSSId,
                    EPSId = nna.EPSId,
                    EPS = nna.EPSId.HasValue
                        ? (await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", nna.EPSId, cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    IPSId = nna.IPSId,
                    IPS = nna.IPSId.HasValue
                        ? (await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", nna.IPSId, cancellationToken))?.FirstOrDefault()?.Nombre
                        : string.Empty,
                    CuidadorNombres = nna.CuidadorNombres,
                    CuidadorParentescoId = nna.CuidadorParentescoId,
                    CuidadorEmail = nna.CuidadorEmail,
                    CuidadorTelefono = nna.CuidadorTelefono,
                };

                reporte.Add(dto);
            }

            return reporte;
        }
    }
}