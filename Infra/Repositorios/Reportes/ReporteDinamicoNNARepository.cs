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
        IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> estadoIngresoEstrategiaService,
        IGenericService<TPEstadoSeguimiento, GenericTPDTO> estadoSeguimientoService
        ) : IReporteDinamicoNNARepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IGenericService<TPCIE10, CIE10DTO> _diagnosticoService = diagnosticoService;
        private readonly IGenericService<TPOrigenReporte, GenericTPDTO> _origenReporteService = origenReporteService;
        private readonly TablaParametricaService _tablaParametricaService = tablaParametricaService;
        private readonly IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> _estadoIngresoEstrategiaService = estadoIngresoEstrategiaService;
        private readonly IGenericService<TPEstadoSeguimiento, GenericTPDTO> _estadoSeguimientoService = estadoSeguimientoService;

        private async Task<string> GetLastSeguimiento(long NNAId)
        {
            var seguimiento = await _context.Seguimientos
                .Where(s => s.NNAId == NNAId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (seguimiento == null)
                return string.Empty;
            var estado = await _origenReporteService.GetByIdAsync(seguimiento.EstadoId, default);

            return estado?.Nombre ?? string.Empty;
        }

        public async Task<List<ReporteDinamicoNNADTO>> GetReporteDinamicoNNAAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            var nnas = await _context.NNAs
                .Where(nna => nna.FechaIngresoEstrategia >= fechaInicio && nna.FechaIngresoEstrategia <= fechaFin)
                .ToListAsync(cancellationToken);

            var reporte = new List<ReporteDinamicoNNADTO>();

            foreach (var nna in nnas)
            {
                try
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
                        Sexo = !string.IsNullOrEmpty(nna.SexoId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "Sexo",
                                nna.SexoId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        TipoIdentificacionId = nna.TipoIdentificacionId,
                        TipoIdentificacion = !string.IsNullOrEmpty(nna.TipoIdentificacionId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "APSTipoIdentificacion",
                                nna.TipoIdentificacionId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        NumeroIdentificacion = nna.NumeroIdentificacion,
                        FechaNotificacionSIVIGILA = nna.FechaNotificacionSIVIGILA,
                        DiagnosticoId = nna.DiagnosticoId,
                        Diagnostico = nna.DiagnosticoId.HasValue
                            ? (await _diagnosticoService.GetByIdAsync(nna.DiagnosticoId ?? 0, cancellationToken))?.Nombre ?? nna.DiagnosticoId.ToString()
                            : string.Empty,
                        OrigenReporteId = nna.OrigenReporteId,
                        OrigenReporte = nna.OrigenReporteId.HasValue
                            ? (await _origenReporteService.GetByIdAsync(nna.OrigenReporteId ?? 0, cancellationToken))?.Nombre ?? nna.OrigenReporteId.ToString()
                            : string.Empty,
                        PaisId = nna.PaisId,
                        Pais = !string.IsNullOrEmpty(nna.PaisId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "Pais",
                                nna.PaisId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        EtniaId = nna.EtniaId,
                        Etnia = !string.IsNullOrEmpty(nna.EtniaId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "GrupoEtnico",
                                nna.EtniaId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        MunicipioNacimientoId = nna.MunicipioNacimientoId,
                        MunicipioNacimiento =
                            !string.IsNullOrEmpty(nna.MunicipioNacimientoId) &&
                            int.TryParse(nna.MunicipioNacimientoId, out int municipioId)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Municipio",
                                    municipioId,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,

                        DepartamentoNacimiento =
                            !string.IsNullOrEmpty(nna.MunicipioNacimientoId) &&
                            nna.MunicipioNacimientoId.Length >= 2 &&
                            int.TryParse(nna.MunicipioNacimientoId.Substring(0, 2), out int codigoDepartamento)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Departamento",
                                    codigoDepartamento,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,

                        GrupoPoblacionId = nna.GrupoPoblacionId,
                        GrupoPoblacion = !string.IsNullOrEmpty(nna.GrupoPoblacionId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "LCETipoPoblacionESPECIAL",
                                nna.GrupoPoblacionId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        ResidenciaOrigenMunicipioId = nna.ResidenciaOrigenMunicipioId,
                        ResidenciaOrigenDepartamento = 
                            !string.IsNullOrEmpty(nna.ResidenciaOrigenMunicipioId) &&
                            nna.ResidenciaOrigenMunicipioId.Length >= 2 &&
                            int.TryParse(nna.ResidenciaOrigenMunicipioId.Substring(0, 2), out int codigoDepartamento2)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Departamento",
                                    codigoDepartamento2,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaOrigenMunicipio =
                            !string.IsNullOrEmpty(nna.ResidenciaOrigenMunicipioId) &&
                            int.TryParse(nna.ResidenciaOrigenMunicipioId, out int municipioId2)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Municipio",
                                    municipioId2,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaOrigenBarrio = nna.ResidenciaOrigenBarrio,
                        ResidenciaOrigenAreaId = nna.ResidenciaOrigenAreaId,
                        AreaProcedencia =
                            !string.IsNullOrEmpty(nna.ResidenciaOrigenAreaId) &&
                            int.TryParse(nna.ResidenciaOrigenAreaId, out int areaId)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo("ZonaTerritorial", areaId, cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaOrigenDireccion = nna.ResidenciaOrigenDireccion,
                        ResidenciaOrigenEstratoId = nna.ResidenciaOrigenEstratoId,
                        ResidenciaActualTelefono = nna.ResidenciaActualTelefono,
                        ResidenciaActualMunicipioId = nna.ResidenciaActualMunicipioId,
                        DepartamentoResidenciaActual =
                            !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId) &&
                            nna.ResidenciaActualMunicipioId.Length >= 2 &&
                            int.TryParse(nna.ResidenciaActualMunicipioId.Substring(0, 2), out int codigoDepartamento3)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Departamento",
                                    codigoDepartamento3,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        MunicipioResidenciaActual =
                            !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId) &&
                            int.TryParse(nna.ResidenciaActualMunicipioId, out int municipioId3)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Municipio",
                                    municipioId3,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        EstadoIngresoEstrategiaId = nna.EstadoIngresoEstrategiaId,
                        EstadoIngresoEstrategia = nna.EstadoIngresoEstrategiaId.HasValue
                            ? (await _estadoIngresoEstrategiaService.GetByIdAsync(nna.EstadoIngresoEstrategiaId ?? 0, cancellationToken))?.Nombre ?? nna.EstadoIngresoEstrategiaId.ToString()
                            : string.Empty,
                        FechaIngresoEstrategia = nna.FechaIngresoEstrategia,
                        TipoRegimenSSId = nna.TipoRegimenSSId,
                        TipoRegimenSS = !string.IsNullOrEmpty(nna.TipoRegimenSSId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "APSRegimenAfiliacion",
                                nna.TipoRegimenSSId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
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
                        CuidadorParentesco = !string.IsNullOrEmpty(nna.CuidadorParentescoId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "RLCPDParentesco",
                                nna.CuidadorParentescoId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        CuidadorEmail = nna.CuidadorEmail,
                        CuidadorTelefono = nna.CuidadorTelefono,
                        TipoSeguimiento = await GetLastSeguimiento(nna.Id)
                    };


                    reporte.Add(dto);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return reporte;
        }
    }
}