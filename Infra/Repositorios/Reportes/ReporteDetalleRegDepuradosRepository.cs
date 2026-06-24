using Core.DTOs.MSTablasParametricas;
using Core.DTOs.Reportes;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Modelos;
using Core.Modelos.TablasParametricas;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Reportes
{
    public class ReporteDetalleRegDepuradosRepository(
        ApplicationDbContext context,
        IGenericService<TPCIE10, CIE10DTO> diagnosticoService,
        IGenericService<TPOrigenReporte, GenericTPDTO> origenReporteService,
        TablaParametricaService tablaParametricaService,
        IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> estadoIngresoEstrategiaService,
        IGenericService<TPEstadoSeguimiento, GenericTPDTO> estadoSeguimientoService,
        IIdentityService identityService
        ) : IReporteDetalleRegDepuradosRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IGenericService<TPCIE10, CIE10DTO> _diagnosticoService = diagnosticoService;
        private readonly IGenericService<TPOrigenReporte, GenericTPDTO> _origenReporteService = origenReporteService;
        private readonly TablaParametricaService _tablaParametricaService = tablaParametricaService;
        private readonly IGenericService<TPEstadoIngresoEstrategia, GenericTPDTO> _estadoIngresoEstrategiaService = estadoIngresoEstrategiaService;
        private readonly IGenericService<TPEstadoSeguimiento, GenericTPDTO> _estadoSeguimientoService = estadoSeguimientoService;
        private readonly IIdentityService _identityService = identityService;
        public async Task<List<ReporteDetalleRegDepuradosDTO>> GetReporteDetalleRegDepuradosAsync(int IdReporteDepuracion, int TipoRegistro, CancellationToken cancellationToken)
        {
            // Obtener los IdNNA desde ReporteDepuracionDetalle
            var idNNAList = await _context.ReporteDepuracionDetalle
                .Where(item =>
                    item.IdReporteDepuracion == IdReporteDepuracion &&
                    (TipoRegistro < 1 || TipoRegistro > 4 || item.TipoRegistro == TipoRegistro))
                .Select(item => item.IdNNA)
                .ToListAsync(cancellationToken);

            // Verificar si la lista está vacía
            List<NNAs> nnas;

            if (idNNAList.Any())
            {
                // Si hay elementos en idNNAList, filtrar en la tabla NNAs
                nnas = await _context.NNAs
                    .Where(nna => idNNAList.Contains(nna.Id))
                    .ToListAsync(cancellationToken);
            }
            else
            {
                // Si idNNAList está vacía, devolver una lista vacía
                nnas = new List<NNAs>();
            }

            var reporte = new List<ReporteDetalleRegDepuradosDTO>();

            foreach (var nna in nnas)
            {
                try
                {
                    var dto = new ReporteDetalleRegDepuradosDTO
                    {
                        Id = nna.Id,
                        FechaNotificacion = nna.FechaNotificacionSIVIGILA,
                        OrigenReporteId = nna.OrigenReporteId,
                        OrigenReporte = nna.OrigenReporteId.HasValue
                            ? (await _origenReporteService.GetByIdAsync(nna.OrigenReporteId ?? 0, cancellationToken))?.Nombre ?? nna.OrigenReporteId.ToString()
                            : string.Empty,
                        PrimerNombre = nna.PrimerNombre,
                        SegundoNombre = nna.SegundoNombre,
                        PrimerApellido = nna.PrimerApellido,
                        SegundoApellido = nna.SegundoApellido,
                        DiagnosticoId = nna.DiagnosticoId,
                        Diagnostico = nna.DiagnosticoId.HasValue
                            ? (await _diagnosticoService.GetByIdAsync(nna.DiagnosticoId ?? 0, cancellationToken))?.Nombre ?? nna.DiagnosticoId.ToString()
                            : string.Empty,
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
                        DepartamentoTratamientoId = nna.DepartamentoTratamientoId,
                        DepartamentoTratamiento =
                            !string.IsNullOrEmpty(nna.DepartamentoTratamientoId) && int.TryParse(nna.DepartamentoTratamientoId, out int departamentoId)
                                ? (await _tablaParametricaService.GetBynomTREFCodigo(
                                    "Departamento",
                                    departamentoId,
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
                        CuidadorParentesco = nna.CuidadorParentescoId != null
                            ? (await _tablaParametricaService.GetBynomTREFStringCodigo(
                                "RLCPDParentesco",
                                nna.CuidadorParentescoId.ToString(),
                                cancellationToken))?.FirstOrDefault()?.Nombre
                            : string.Empty,
                        CuidadorEmail = nna.CuidadorEmail,
                        CuidadorTelefono = nna.CuidadorTelefono,
                        Agente = await GetAgente(nna.Id),
                        EstadoNNAId = nna.estadoId,
                        Estado = await _context.TPEstadoNNA
                            .Where(e => e.Id == nna.estadoId)
                            .Select(e => e.Nombre)
                            .FirstOrDefaultAsync(cancellationToken)
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
        private async Task<string> GetAgente(long NNAId)
        {
            var seguimiento = await _context.Seguimientos
                .Where(s => s.NNAId == NNAId)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            var agente = (seguimiento == null) ? string.Empty : await GetAgenteById(seguimiento.UsuarioId);

            return agente;
        }

        private async Task<string> GetAgenteById(string? usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId))
                return string.Empty;

            var users = await _identityService.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.id == usuarioId);

            // Validar que user no sea null antes de acceder a fullName
            return user.fullName;
        }

    }
}
