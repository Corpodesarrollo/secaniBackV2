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
    public class ReporteDinamicoAlertasRepository(
        ApplicationDbContext context,
        IGenericService<TPCIE10, CIE10DTO> diagnosticoService,
        TablaParametricaService tablaParametricaService,
        IGenericService<TPOrigenReporte, GenericTPDTO> origenReporteService,
        IGenericService<TPCausaInasistencia, GenericTPDTO> causaInasistenciaReporteService,
        IGenericService<TPCategoriaAlerta, GenericTPDTO> categoriaAlertaService,
        IGenericService<TPSubCategoriaAlerta, GenericTPDTO> subCategoriaAlertaService,
        IGenericService<TPEstadoAlerta, GenericTPDTO> estadoAlertaService,
        IIdentityService identityService
    ) : IReporteDinamicoAlertasRepository
    {
        private readonly IGenericService<TPCIE10, CIE10DTO> _diagnosticoService = diagnosticoService;
        private readonly ApplicationDbContext _context = context;
        private readonly TablaParametricaService _tablaParametricaService = tablaParametricaService ?? throw new ArgumentNullException(nameof(tablaParametricaService));
        private readonly IGenericService<TPOrigenReporte, GenericTPDTO> _origenReporteService = origenReporteService ?? throw new ArgumentNullException(nameof(origenReporteService));
        private readonly IIdentityService _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
        private readonly IGenericService<TPCausaInasistencia, GenericTPDTO> _causaInasistenciaReporteService = causaInasistenciaReporteService ?? throw new ArgumentNullException(nameof(causaInasistenciaReporteService));
        private readonly IGenericService<TPCategoriaAlerta, GenericTPDTO> _categoriaAlertaService = categoriaAlertaService ?? throw new ArgumentNullException(nameof(categoriaAlertaService));
        private readonly IGenericService<TPSubCategoriaAlerta, GenericTPDTO> _subCategoriaAlertaService = subCategoriaAlertaService ?? throw new ArgumentNullException(nameof(subCategoriaAlertaService));
        private readonly IGenericService<TPEstadoAlerta, GenericTPDTO> _estadoAlertaService = estadoAlertaService ?? throw new ArgumentNullException(nameof(estadoAlertaService));

        public async Task<List<ReporteDinamicoAlertasDTO>> GetReporteDinamicoAlertasAsync(DateTime FechaInicio, DateTime FechaFin, CancellationToken cancellationToken)
        {
            // Periodo inclusivo
            var fin = FechaFin.Date.AddDays(1).AddTicks(-1);

            List<AlertaSeguimientoDTO> alertas = (from al in _context.AlertaSeguimientos
                                                  join alerta in _context.Alertas on al.AlertaId equals alerta.Id
                                                  where al.UltimaFechaSeguimiento.HasValue
                                                     && al.UltimaFechaSeguimiento.Value >= FechaInicio
                                                     && al.UltimaFechaSeguimiento.Value <= fin
                                                  select new AlertaSeguimientoDTO()
                                                  {
                                                      AlertaSeguimientoId = al.Id,
                                                      AlertaId = al.AlertaId,
                                                      EstadoId = al.EstadoId,
                                                      NombreAlerta = alerta.Descripcion,
                                                      Observaciones = al.Observaciones,
                                                      SeguimientoId = al.SeguimientoId,
                                                      UltimaFechaSeguimiento = al.UltimaFechaSeguimiento!.Value
                                                  }).ToList();

            var reporte = new List<ReporteDinamicoAlertasDTO>();

            foreach (var item in alertas)
            {
                try
                {
                    var seguimiento = _context.Seguimientos.FirstOrDefault(s => s.Id == item.SeguimientoId);
                    var nna = seguimiento != null ? _context.NNAs.FirstOrDefault(n => n.Id == seguimiento.NNAId) : null;
                    var alerta = await _context.Alertas.FirstOrDefaultAsync(a => a.Id == item.AlertaId, cancellationToken);
                    // Lookup directo: el campo en TPSubCategoriaAlerta se llama SubCategoriaAlerta, no Nombre.
                    var subAlertaModel = (alerta != null) ? _context.TPSubCategoriaAlerta.FirstOrDefault(s => s.Id == alerta.SubcategoriaId && !s.IsDeleted) : null;
                    var notificacion = ObtenerUltimaNotificacion(item.AlertaSeguimientoId);
                    int tratamientoCausasInasistenciaId = nna != null && int.TryParse(nna.TratamientoCausasInasistenciaId?.ToString(), out int id) ? id : 0;

                    if (nna != null)
                    {
                        var dto = new ReporteDinamicoAlertasDTO
                        {
                            //Seguimiento
                            FechaNotificacion = item.UltimaFechaSeguimiento,
                            FechaResolucion = (item.EstadoId == 6) ? (DateTime?)item.UltimaFechaSeguimiento : null,
                            Notificaciones = _context.NotificacionesEntidad.Count(n => n.AlertaSeguimientoId == item.AlertaSeguimientoId && (n.IsDeleted == false || n.IsDeleted == null)),

                            //NNA
                            NNAId = nna.Id,
                            PrimerNombre = nna.PrimerNombre,
                            SegundoNombre = nna.SegundoNombre,
                            PrimerApellido = nna.PrimerApellido,
                            SegundoApellido = nna.SegundoApellido,
                            NombreCompleto = string.Join(" ", new[] { nna.PrimerNombre, nna.SegundoNombre, nna.PrimerApellido, nna.SegundoApellido }.Where(s => !string.IsNullOrWhiteSpace(s))),
                            Observacion = item.Observaciones ?? string.Empty,
                            DiagnosticoId = nna.DiagnosticoId,
                            Diagnostico = nna.DiagnosticoId.HasValue
                                ? (await _diagnosticoService.GetByIdAsync(nna.DiagnosticoId ?? 0, cancellationToken))?.Nombre ?? string.Empty
                                : string.Empty,
                            Edad = nna.FechaNacimiento.HasValue
                                ? (int)((DateTime.Now - nna.FechaNacimiento.Value).TotalDays / 365.25)
                                : 0,
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
                            ResidenciaActualDireccion = nna.ResidenciaActualDireccion,
                            TrasladosQuienAsumioCostosTraslado = nna.TrasladosQuienAsumioCostosTraslado,
                            TrasladosQuienAsumioCostosVivienda = nna.TrasladosQuienAsumioCostosVivienda,
                            TratamientoHaDejadodeAsistir = nna.TratamientoHaDejadodeAsistir,
                            TipoSeguimiento = (seguimiento == null) ? string.Empty : (_context.TPEstadoSeguimiento.FirstOrDefault(e => e.Id == seguimiento.EstadoId)?.Nombre ?? string.Empty),
                            Agente = await GetAgente(seguimiento.UsuarioId),
                            EPSId = nna.EAPBId,
                            EPS = nna.EAPBId.HasValue
                                ? (_context.TPEAPB.FirstOrDefault(e => e.Id == nna.EAPBId.Value)?.Nombre ?? string.Empty)
                                : string.Empty,
                            // Email del contacto del NNA (ContactoNNAs.Email); fallback a NNAs.CuidadorEmail
                            CuidadorEmail = _context.ContactoNNAs
                                .Where(c => c.NNAId == nna.Id && !c.IsDeleted && !string.IsNullOrEmpty(c.Email))
                                .OrderBy(c => c.Id)
                                .Select(c => c.Email)
                                .FirstOrDefault() ?? nna.CuidadorEmail,
                            TratamientoCuantoTiemposinAsistir = nna.TratamientoCuantoTiemposinAsistir,
                            TratamientoUnidadMedidaIdTiempoId = nna.TratamientoUnidadMedidaIdTiempoId,
                            TratamientoUnidadMedidaTiempo = nna.TratamientoUnidadMedidaIdTiempoId, //pendiente de la tabla parametrica
                            TratamientoCausasInasistenciaId = tratamientoCausasInasistenciaId.ToString(),
                            TratamientoCausasInasistencia = (await _causaInasistenciaReporteService.GetByIdAsync(tratamientoCausasInasistenciaId, default))?.Nombre ?? string.Empty,
                            CategoriaAlerta = (alerta != null) ? alerta.Descripcion ?? string.Empty : string.Empty,
                            SubCategoriaAlerta = subAlertaModel?.SubCategoriaAlerta ?? string.Empty,
                            EstadoAlerta = (alerta == null) ? string.Empty : (await _estadoAlertaService.GetByIdAsync(item.EstadoId, default))?.Nombre ?? string.Empty,
                            TratamientoEstudiaActualmente = nna.TratamientoEstudiaActualmente,
                            TratamientoHaDejadodeAsistirColegio = nna.TratamientoHaDejadodeAsistirColegio,
                            TratamientoTiempoInasistenciaColegio = nna.TratamientoTiempoInasistenciaColegio,
                            TratamientoTiempoInasistenciaUnidadMedidaId = nna.TratamientoTiempoInasistenciaUnidadMedidaId,
                            TratamientoTiempoInasistenciaUnidadMedida = nna.TratamientoTiempoInasistenciaUnidadMedidaId, //pendiente de la tabla parametrica
                            RespuestaEntidad = ObtenerRespuestaTexto(item.AlertaSeguimientoId),
                            FechaRespuesta = ObtenerRespuestaFecha(item.AlertaSeguimientoId),
                            TratamientoHaSidoInformadoClaramente = nna.TratamientoHaSidoInformadoClaramente,
                            TrasladosHaSolicitadoApoyoFundacion = nna.TrasladosHaSolicitadoApoyoFundacion,
                            TrasladosNombreFundacion = nna.TrasladosNombreFundacion,
                            TrasladosApoyoRecibidoxFundacion = nna.TrasladosApoyoRecibidoxFundacion,
                            TrasladosHaSidoTrasladadodeInstitucion = nna.TrasladosHaSidoTrasladadodeInstitucion,
                        };
                    reporte.Add(dto);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return reporte;
        }

        public Notificacion? ObtenerUltimaNotificacion(long alertaSeguimientoId)
        {
            var ultimaNotificacion = _context.Notificacions
                .Where(n => n.AlertaSeguimientoId == alertaSeguimientoId)
                .ToList() // Materializa la consulta en memoria
                .LastOrDefault();

            // Toma el primer registro

            return ultimaNotificacion;
        }

        // Respuesta entidad real: RespuestasAlerta.IdAlerta == AlertaSeguimientoId
        // (NotificacionEntidadId=0 en datos reales, no se usa)
        private string ObtenerRespuestaTexto(long alertaSeguimientoId)
        {
            var ultima = _context.RespuestasAlerta
                .Where(r => r.IdAlerta == alertaSeguimientoId && (r.IsDeleted == false || r.IsDeleted == null))
                .OrderByDescending(r => r.DateCreated)
                .FirstOrDefault();
            return StripHtml(ultima?.Respuesta);
        }

        private DateTime? ObtenerRespuestaFecha(long alertaSeguimientoId)
        {
            var ultima = _context.RespuestasAlerta
                .Where(r => r.IdAlerta == alertaSeguimientoId && (r.IsDeleted == false || r.IsDeleted == null))
                .OrderByDescending(r => r.DateCreated)
                .FirstOrDefault();
            return ultima?.DateCreated;
        }

        // Quill editor guarda HTML; el reporte requiere texto plano.
        private static string StripHtml(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            var sinTags = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", string.Empty);
            return System.Net.WebUtility.HtmlDecode(sinTags).Trim();
        }

        private async Task<string> GetAgente(string? usuarioId)
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
