using Core.DTOs.MSTablasParametricas;
using Core.DTOs.Reportes;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos;
using Core.Modelos.TablasParametricas;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders.Testing;

namespace Infra.Repositorios.Reportes
{
    public class ReporteDinamicoSeguimientoRepository(
        ApplicationDbContext context,
        IGenericService<TPCIE10, CIE10DTO> diagnosticoService,
        IGenericService<TPOrigenReporte, GenericTPDTO> origenReporteService,
        TablaParametricaService tablaParametricaService,
        IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> estadoIngresoEstrategiaService,
        IGenericService<TPEstadoSeguimiento, GenericTPDTO> estadoSeguimientoService
        ) : IReporteDinamicoSeguimientoRepository
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

        public async Task<List<ReporteDinamicoSeguimientoDTO>> GetReporteDinamicoSeguimientoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
        {
            var seguimientos = await _context.Seguimientos
                .Where(item => item.FechaSeguimiento >= fechaInicio && item.FechaSeguimiento <= fechaFin)
                .ToListAsync(cancellationToken);

            var reporte = new List<ReporteDinamicoSeguimientoDTO>();

            foreach (var item in seguimientos)
            {
                try
                {
                    var nna = await _context.NNAs.FirstOrDefaultAsync(n => n.Id == item.NNAId, cancellationToken);
                    if (nna == null) continue;
                    var dto = new ReporteDinamicoSeguimientoDTO
                    {
                        //Seguimiento
                        SeguimientoId = item.Id,
                        FechaSeguimiento = item.FechaSeguimiento,
                        ObservacionesSolicitante = item.ObservacionesSolicitante,
                        ObservacionAgente = item.ObservacionAgente,
                        EstadoId = item.EstadoId,
                        Estado = (await _origenReporteService.GetByIdAsync(item.EstadoId, default))?.Nombre ?? string.Empty,

                        //NNA
                        NNAId = nna.Id,
                        PrimerNombre = nna.PrimerNombre,
                        SegundoNombre = nna.SegundoNombre,
                        PrimerApellido = nna.PrimerApellido,
                        SegundoApellido = nna.SegundoApellido,
                        DiagnosticoId = nna.DiagnosticoId,
                        Diagnostico = nna.DiagnosticoId.HasValue
                            ? (await _diagnosticoService.GetByIdAsync(nna.DiagnosticoId ?? 0, cancellationToken))?.Nombre ?? nna.DiagnosticoId.ToString()
                            : string.Empty,
                        TipoIdentificacionId = nna.TipoIdentificacionId,
                        TipoIdentificacion = !string.IsNullOrEmpty(nna.TipoIdentificacionId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "APSTipoIdentificacion",
                                nna.TipoIdentificacionId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        NumeroIdentificacion = nna.NumeroIdentificacion,
                        TipoRegimenSSId = nna.TipoRegimenSSId,
                        TipoRegimenSS = !string.IsNullOrEmpty(nna.TipoRegimenSSId)
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "APSRegimenAfiliacion",
                                nna.TipoRegimenSSId,
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        EAPBId = nna.EAPBId,
                        EAPB = nna.EAPBId.HasValue
                            ? (await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", nna.EAPBId, cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        FechaConsultaDiagnostico = nna.FechaConsultaDiagnostico,
                        FechaDiagnostico = nna.FechaDiagnostico,
                        MotivoNoDiagnosticoId = nna.MotivoNoDiagnosticoId,
                        MotivoNoDiagnostico ="",
                        MotivoNoDiagnosticoOtro = nna.MotivoNoDiagnosticoOtro,
                        FechaInicioTratamiento = nna.FechaInicioTratamiento,
                        IPSId = nna.IPSId,
                        IPS = nna.IPSId.HasValue
                            ? (await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", nna.IPSId, cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        Recaida = nna.Recaida,
                        CantidadRecaidas = nna.CantidadRecaidas,
                        FechaUltimaRecaida = nna.FechaUltimaRecaida,
                        TrasladosHaSidoTrasladadodeInstitucion = nna.TrasladosHaSidoTrasladadodeInstitucion,
                        ResidenciaActualMunicipioId = nna.ResidenciaActualMunicipioId,
                        ResidenciaActualDepartamento =
                            !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId) &&
                            nna.ResidenciaActualMunicipioId.Length >= 2 &&
                            int.TryParse(nna.ResidenciaActualMunicipioId.Substring(0, 2), out int codigoDepartamento3)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Departamento",
                                    codigoDepartamento3,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaActualMunicipio =
                            !string.IsNullOrEmpty(nna.ResidenciaActualMunicipioId) &&
                            int.TryParse(nna.ResidenciaActualMunicipioId, out int municipioId3)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Municipio",
                                    municipioId3,
                                    cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaActualBarrio = nna.ResidenciaActualBarrio,
                        ResidenciaActualDireccion = nna.ResidenciaActualDireccion,
                        ResidenciaActualAreaId = nna.ResidenciaActualAreaId,
                        ResidenciaActualArea =
                            !string.IsNullOrEmpty(nna.ResidenciaActualAreaId) &&
                            int.TryParse(nna.ResidenciaActualAreaId, out int areaId)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo("ZonaTerritorial", areaId, cancellationToken))?.FirstOrDefault()?.Nombre
                                : string.Empty,
                        ResidenciaActualEstratoId = nna.ResidenciaActualEstratoId,
                        TrasladoTieneCapacidadEconomica = nna.TrasladoTieneCapacidadEconomica,
                        TrasladoEAPBSuministroApoyo = nna.TrasladoEAPBSuministroApoyo,
                        TrasladosServiciosdeApoyoOportunos = nna.TrasladosServiciosdeApoyoOportunos,
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