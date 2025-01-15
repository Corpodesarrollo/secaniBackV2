using ClosedXML.Excel;
using Core.DTOs;
using Core.DTOs.MSTablasParametricas;
using Core.DTOs.Reportes;
using Core.Interfaces.MSTablasParametricas;
using Core.Interfaces.Repositorios.Reportes;
using Core.Modelos.TablasParametricas;
using Core.Services.MSTablasParametricas;
using Microsoft.EntityFrameworkCore;

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
                        //EAPB = !string.IsNullOrEmpty(nna.EAPBId)
                        //    ? (await _tablaParametricaService.GetBynomTREFCodigo("CodigoEAPByNit", nna.EAPBId, cancellationToken))?.FirstOrDefault()?.Nombre
                        //    : string.Empty,
                        FechaConsultaDiagnostico = nna.FechaConsultaDiagnostico,
                        FechaDiagnostico = nna.FechaDiagnostico,
                        MotivoNoDiagnosticoId = nna.MotivoNoDiagnosticoId,
                        MotivoNoDiagnostico = "",
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

        public async Task<List<ReporteCasosEAPBDto>> GetReporteCasosEAPBAsync(ReporteCasosEAPBRequestDto request)
        {
            var result = await (from s in _context.Seguimientos
                                join nna in _context.NNAs on s.NNAId equals nna.Id
                                join e in _context.TPEstadoNNA on nna.estadoId equals e.Id
                                where s.FechaSeguimiento >= request.FechaInicial && s.FechaSeguimiento <= request.FechaFinal
                                && nna.EAPBId == request.EAPB
                                select new
                                {
                                    s.FechaSeguimiento,
                                    nombreNNA = $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}",
                                    nna.FechaNacimiento,
                                    sexo = nna.SexoId == "1" ? "Masculino" : nna.SexoId == "2" ? "Femenino" : "No definido",
                                    estado = e.Nombre,
                                }).ToListAsync();

            if (!string.IsNullOrEmpty(request.Buscar))
            {
                if (DateTime.TryParse(request.Buscar, out var fecha))
                    result = result.Where(r => r.FechaSeguimiento == fecha).ToList();
                else
                    result = result.Where(r => r.nombreNNA.Contains(request.Buscar) || r.sexo.Contains(request.Buscar) || r.estado.Contains(request.Buscar)).ToList();
            }

            var reporte = new List<ReporteCasosEAPBDto>();
            result.ForEach(r =>
            {
                var edad = CalcularEdad(r.FechaNacimiento ?? DateTime.Now);
                var tiempoTranscurrido = (DateTime.Now - r.FechaSeguimiento)?.Days ?? 0;
                reporte.Add(new ReporteCasosEAPBDto
                {
                    FechaNotificacion = r.FechaSeguimiento,
                    NombreNNA = r.nombreNNA,
                    Edad = edad,
                    Sexo = r.sexo,
                    TiempoTranscurrido = tiempoTranscurrido,
                    Estado = r.estado
                });
            });

            return reporte;
        }

        public async Task<List<ReporteCasosEAPBDto>> GetReporteCasosEntidadAsync(ReporteCasosEntidadRequestDto request)
        {
            var result = await (from s in _context.Seguimientos
                                join nna in _context.NNAs on s.NNAId equals nna.Id
                                join e in _context.TPEstadoNNA on nna.estadoId equals e.Id
                                where s.FechaSeguimiento >= request.FechaInicial && s.FechaSeguimiento <= request.FechaFinal
                                && nna.EPSId == request.Entidad
                                select new
                                {
                                    s.FechaSeguimiento,
                                    nombreNNA = $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}",
                                    nna.FechaNacimiento,
                                    sexo = nna.SexoId == "1" ? "Masculino" : nna.SexoId == "2" ? "Femenino" : "No definido",
                                    estado = e.Nombre,
                                }).ToListAsync();

            if (!string.IsNullOrEmpty(request.Buscar))
            {
                if (DateTime.TryParse(request.Buscar, out var fecha))
                    result = result.Where(r => r.FechaSeguimiento == fecha).ToList();
                else
                    result = result.Where(r => r.nombreNNA.Contains(request.Buscar) || r.sexo.Contains(request.Buscar) || r.estado.Contains(request.Buscar)).ToList();
            }

            var reporte = new List<ReporteCasosEAPBDto>();
            result.ForEach(r =>
            {
                var edad = CalcularEdad(r.FechaNacimiento ?? DateTime.Now);
                var tiempoTranscurrido = (DateTime.Now - r.FechaSeguimiento)?.Days ?? 0;
                reporte.Add(new ReporteCasosEAPBDto
                {
                    FechaNotificacion = r.FechaSeguimiento,
                    NombreNNA = r.nombreNNA,
                    Edad = edad,
                    Sexo = r.sexo,
                    TiempoTranscurrido = tiempoTranscurrido,
                    Estado = r.estado
                });
            });

            return reporte;
        }

        public async Task<string> GetReporteCasosEAPBExcelAsync(ReporteCasosEAPBRequestDto request)
        {
            var result = from s in _context.Seguimientos
                         join nna in _context.NNAs on s.NNAId equals nna.Id
                         join e in _context.TPEstadoNNA on nna.estadoId equals e.Id

                         join c in _context.TPCategoriaAlerta on nna.CategoriaAlertaId equals c.Id
                         join sa in _context.AlertaSeguimientos on s.Id equals sa.SeguimientoId
                         join a in _context.Alertas on sa.AlertaId equals a.Id
                         join sc in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sc.Id

                         join r in _context.RespuestasAlerta on a.Id equals r.IdAlerta

                         where s.FechaSeguimiento >= request.FechaInicial && s.FechaSeguimiento <= request.FechaFinal
                         && nna.EAPBId == request.EAPB
                         select new
                         {
                             s.FechaSeguimiento,
                             nombreNNA = $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}",
                             nna.FechaNacimiento,
                             sexo = nna.SexoId == "1" ? "Masculino" : nna.SexoId == "2" ? "Femenino" : "No definido",
                             CategoriaAlerta = c.Nombre,
                             FechaEnvioRespuesta = r.DateCreated,
                             //DepartamentoProcedencia = nna.ResidenciaOrigenDepartamento,
                             DireccionProcedencia = nna.ResidenciaOrigenDireccion,
                             Nacionalidad = nna.PaisId,
                             SubcategoriaAlerta = sc.SubCategoriaAlerta,
                             r.Respuesta,
                             MunicipioProcedencia = nna.ResidenciaOrigenMunicipioId,
                             //DepartamentoActual = nna.ResidenciaActualDepartamento,
                             Etnia = nna.EtniaId,
                             Observaciones = s.ObservacionAgente,
                             TipoIdentificacion = nna.TipoIdentificacionId,
                             BarrioProcedencia = nna.ResidenciaOrigenBarrio,
                             EstadoNNA = e.Nombre,
                             nna.NumeroIdentificacion,
                             AreaProcedencia = nna.ResidenciaOrigenAreaId,
                             RegimenAfiliacion = nna.TipoRegimenSSId,
                             DiagnosticoNNA = nna.DiagnosticoId,
                             IpsPrimaria = nna.IPSId
                         };


            if (!string.IsNullOrEmpty(request.Buscar))
            {
                if (DateTime.TryParse(request.Buscar, out var fecha))
                    result = result.Where(r => r.FechaSeguimiento == fecha);
                else
                    result = result.Where(r => r.nombreNNA.Contains(request.Buscar) || r.sexo.Contains(request.Buscar) || r.EstadoNNA.Contains(request.Buscar));
            }

            var lista = await result.ToListAsync();

            var reporte = new List<ReporteCasosEAPBExcelDto>();
            lista.ForEach(r =>
            {
                var edad = CalcularEdad(r.FechaNacimiento ?? DateTime.Now);
                var tiempoTranscurrido = (DateTime.Now - r.FechaSeguimiento)?.Days ?? 0;
                reporte.Add(new ReporteCasosEAPBExcelDto
                {
                    FechaNotificacion = r.FechaSeguimiento,
                    NombreNNA = r.nombreNNA,
                    Edad = edad,
                    Sexo = r.sexo,
                    TiempoTranscurrido = tiempoTranscurrido,
                    Estado = r.EstadoNNA,
                    CategoriaAlerta = r.CategoriaAlerta,
                    FechaEnvioRespuesta = r.FechaEnvioRespuesta,
                    //DepartamentoProcedencia = r.DepartamentoProcedencia,
                    DireccionProcedencia = r.DireccionProcedencia,
                    Nacionalidad = r.Nacionalidad,
                    SubcategoriaAlerta = r.SubcategoriaAlerta,
                    Respuesta = r.Respuesta,
                    MunicipioProcedencia = r.MunicipioProcedencia,
                    //DepartamentoActual = r.DepartamentoActual,
                    Etnia = r.Etnia,
                    Observaciones = r.Observaciones,
                    TipoIdentificacion = r.TipoIdentificacion,
                    BarrioProcedencia = r.BarrioProcedencia,
                    EstadoNNA = r.EstadoNNA,
                    NumeroIdentificacion = r.NumeroIdentificacion,
                    AreaProcedencia = r.AreaProcedencia,
                    RegimenAfiliacion = r.RegimenAfiliacion,
                    DiagnosticoNNA = r.DiagnosticoNNA,
                    IpsPrimaria = r.IpsPrimaria
                });
            });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte Casos EAPB");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Fecha Notificación";
            worksheet.Cell(currentRow, 2).Value = "Nombre NNA";
            worksheet.Cell(currentRow, 3).Value = "Edad";
            worksheet.Cell(currentRow, 4).Value = "Sexo";
            worksheet.Cell(currentRow, 5).Value = "Tiempo Transcurrido";
            worksheet.Cell(currentRow, 6).Value = "Estado";

            var pos = 7;
            if (request.CategoriaAlerta)
            {
                worksheet.Cell(currentRow, pos).Value = "Categoria Alerta";
                pos++;
            }

            if (request.FechaEnvioRespuesta)
            {
                worksheet.Cell(currentRow, pos).Value = "Fecha Envio Respuesta";
                pos++;
            }

            if (request.DepartamentoProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Departamento Procedencia";
                pos++;
            }

            if (request.DireccionProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Direccion Procedencia";
                pos++;
            }

            if (request.Nacionalidad)
            {
                worksheet.Cell(currentRow, pos).Value = "Nacionalidad";
                pos++;
            }

            if (request.SubcategoriaAlerta)
            {
                worksheet.Cell(currentRow, pos).Value = "Subcategoria Alerta";
                pos++;
            }

            if (request.Respuesta)
            {
                worksheet.Cell(currentRow, pos).Value = "Respuesta";
                pos++;
            }

            if (request.MunicipioProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Municipio Procedencia";
                pos++;
            }

            if (request.DepartamentoActual)
            {
                worksheet.Cell(currentRow, pos).Value = "Departamento Actual";
                pos++;
            }

            if (request.Etnia)
            {
                worksheet.Cell(currentRow, pos).Value = "Etnia";
                pos++;
            }

            if (request.Observaciones)
            {
                worksheet.Cell(currentRow, pos).Value = "Observaciones";
                pos++;
            }

            if (request.TipoIdentificacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Tipo Identificación";
                pos++;
            }

            if (request.BarrioProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Barrio Procedencia";
                pos++;
            }

            if (request.EstadoNNA)
            {
                worksheet.Cell(currentRow, pos).Value = "Estado NNA";
                pos++;
            }

            if (request.NumeroIdentificacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Numero Identificación";
                pos++;
            }

            if (request.AreaProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Area Procedencia";
                pos++;
            }

            if (request.RegimenAfiliacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Regimen Afiliacion";
                pos++;
            }

            if (request.DiagnosticoNNA)
            {
                worksheet.Cell(currentRow, pos).Value = "Diagnostico NNA";
                pos++;
            }

            if (request.IpsPrimaria)
            {
                worksheet.Cell(currentRow, pos).Value = "Ips Primaria";
                pos++;
            }

            foreach (var item in reporte)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.FechaNotificacion;
                worksheet.Cell(currentRow, 2).Value = item.NombreNNA;
                worksheet.Cell(currentRow, 3).Value = item.Edad;
                worksheet.Cell(currentRow, 4).Value = item.Sexo;
                worksheet.Cell(currentRow, 5).Value = item.TiempoTranscurrido;
                worksheet.Cell(currentRow, 6).Value = item.Estado;
                pos = 7;
                if (request.CategoriaAlerta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.CategoriaAlerta;
                    pos++;
                }
                if (request.FechaEnvioRespuesta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.FechaEnvioRespuesta;
                    pos++;
                }
                if (request.DepartamentoProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DepartamentoProcedencia;
                    pos++;
                }
                if (request.DireccionProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DireccionProcedencia;
                    pos++;
                }
                if (request.Nacionalidad)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Nacionalidad;
                    pos++;
                }
                if (request.SubcategoriaAlerta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.SubcategoriaAlerta;
                    pos++;
                }
                if (request.Respuesta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Respuesta;
                    pos++;
                }
                if (request.MunicipioProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.MunicipioProcedencia;
                    pos++;
                }
                if (request.DepartamentoActual)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DepartamentoActual;
                    pos++;
                }
                if (request.Etnia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Etnia;
                    pos++;
                }
                if (request.Observaciones)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Observaciones;
                    pos++;
                }
                if (request.TipoIdentificacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.TipoIdentificacion;
                    pos++;
                }
                if (request.BarrioProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.BarrioProcedencia;
                    pos++;
                }
                if (request.EstadoNNA)
                {
                    worksheet.Cell(currentRow, pos).Value = item.EstadoNNA;
                    pos++;
                }
                if (request.NumeroIdentificacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.NumeroIdentificacion;
                    pos++;
                }
                if (request.AreaProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.AreaProcedencia;
                    pos++;
                }
                if (request.RegimenAfiliacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.RegimenAfiliacion;
                    pos++;
                }
                if (request.DiagnosticoNNA)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DiagnosticoNNA;
                    pos++;
                }
                if (request.IpsPrimaria)
                {
                    worksheet.Cell(currentRow, pos).Value = item.IpsPrimaria;
                    pos++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return Convert.ToBase64String(content);
        }

        public async Task<string> GetReporteCasosEntidadExcelAsync(ReporteCasosEntidadRequestDto request)
        {
            var result = from s in _context.Seguimientos
                         join nna in _context.NNAs on s.NNAId equals nna.Id
                         join e in _context.TPEstadoNNA on nna.estadoId equals e.Id

                         join c in _context.TPCategoriaAlerta on nna.CategoriaAlertaId equals c.Id
                         join sa in _context.AlertaSeguimientos on s.Id equals sa.SeguimientoId
                         join a in _context.Alertas on sa.AlertaId equals a.Id
                         join sc in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sc.Id

                         join r in _context.RespuestasAlerta on a.Id equals r.IdAlerta

                         where s.FechaSeguimiento >= request.FechaInicial && s.FechaSeguimiento <= request.FechaFinal
                         && nna.EPSId == request.Entidad
                         select new
                         {
                             s.FechaSeguimiento,
                             nombreNNA = $"{nna.PrimerNombre ?? ""} {nna.SegundoNombre ?? ""} {nna.PrimerApellido ?? ""} {nna.SegundoApellido ?? ""}",
                             nna.FechaNacimiento,
                             sexo = nna.SexoId == "1" ? "Masculino" : nna.SexoId == "2" ? "Femenino" : "No definido",
                             CategoriaAlerta = c.Nombre,
                             FechaEnvioRespuesta = r.DateCreated,
                             //DepartamentoProcedencia = nna.ResidenciaOrigenDepartamento,
                             DireccionProcedencia = nna.ResidenciaOrigenDireccion,
                             SubcategoriaAlerta = sc.SubCategoriaAlerta,
                             r.Respuesta,
                             MunicipioProcedencia = nna.ResidenciaOrigenMunicipioId,
                             //DepartamentoActual = nna.ResidenciaActualDepartamento,
                             Etnia = nna.EtniaId,
                             Observaciones = s.ObservacionAgente,
                             TipoIdentificacion = nna.TipoIdentificacionId,
                             BarrioProcedencia = nna.ResidenciaOrigenBarrio,
                             EstadoNNA = e.Nombre,
                             nna.NumeroIdentificacion,
                             AreaProcedencia = nna.ResidenciaOrigenAreaId,
                             RegimenAfiliacion = nna.TipoRegimenSSId,
                             eapb = nna.EAPBId,
                         };


            if (!string.IsNullOrEmpty(request.Buscar))
            {
                if (DateTime.TryParse(request.Buscar, out var fecha))
                    result = result.Where(r => r.FechaSeguimiento == fecha);
                else
                    result = result.Where(r => r.nombreNNA.Contains(request.Buscar) || r.sexo.Contains(request.Buscar) || r.EstadoNNA.Contains(request.Buscar));
            }

            var lista = await result.ToListAsync();

            var reporte = new List<ReporteCasosEntidadExcelDto>();
            lista.ForEach(r =>
            {
                var edad = CalcularEdad(r.FechaNacimiento ?? DateTime.Now);
                var tiempoTranscurrido = (DateTime.Now - r.FechaSeguimiento)?.Days ?? 0;
                reporte.Add(new ReporteCasosEntidadExcelDto
                {
                    FechaNotificacion = r.FechaSeguimiento,
                    NombreNNA = r.nombreNNA,
                    Edad = edad,
                    Sexo = r.sexo,
                    TiempoTranscurrido = tiempoTranscurrido,
                    Estado = r.EstadoNNA,
                    CategoriaAlerta = r.CategoriaAlerta,
                    FechaEnvioRespuesta = r.FechaEnvioRespuesta,
                    //DepartamentoProcedencia = r.DepartamentoProcedencia,
                    DireccionProcedencia = r.DireccionProcedencia,
                    SubcategoriaAlerta = r.SubcategoriaAlerta,
                    Respuesta = r.Respuesta,
                    MunicipioProcedencia = r.MunicipioProcedencia,
                    //DepartamentoActual = r.DepartamentoActual,
                    Observaciones = r.Observaciones,
                    TipoIdentificacion = r.TipoIdentificacion,
                    BarrioProcedencia = r.BarrioProcedencia,
                    EstadoNNA = r.EstadoNNA,
                    NumeroIdentificacion = r.NumeroIdentificacion,
                    AreaProcedencia = r.AreaProcedencia,
                    RegimenAfiliacion = r.RegimenAfiliacion,
                    EAPB = r.eapb
                });
            });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reporte Casos EAPB");
            var currentRow = 1;
            worksheet.Cell(currentRow, 1).Value = "Fecha Notificación";
            worksheet.Cell(currentRow, 2).Value = "Nombre NNA";
            worksheet.Cell(currentRow, 3).Value = "Edad";
            worksheet.Cell(currentRow, 4).Value = "Sexo";
            worksheet.Cell(currentRow, 5).Value = "Tiempo Transcurrido";
            worksheet.Cell(currentRow, 6).Value = "Estado";

            var pos = 7;
            if (request.CategoriaAlerta)
            {
                worksheet.Cell(currentRow, pos).Value = "Categoria Alerta";
                pos++;
            }

            if (request.FechaEnvioRespuesta)
            {
                worksheet.Cell(currentRow, pos).Value = "Fecha Envio Respuesta";
                pos++;
            }

            if (request.DepartamentoProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Departamento Procedencia";
                pos++;
            }

            if (request.DireccionProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Direccion Procedencia";
                pos++;
            }

            if (request.SubcategoriaAlerta)
            {
                worksheet.Cell(currentRow, pos).Value = "Subcategoria Alerta";
                pos++;
            }

            if (request.Respuesta)
            {
                worksheet.Cell(currentRow, pos).Value = "Respuesta";
                pos++;
            }

            if (request.MunicipioProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Municipio Procedencia";
                pos++;
            }

            if (request.DepartamentoActual)
            {
                worksheet.Cell(currentRow, pos).Value = "Departamento Actual";
                pos++;
            }

            if (request.Observaciones)
            {
                worksheet.Cell(currentRow, pos).Value = "Observaciones";
                pos++;
            }

            if (request.TipoIdentificacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Tipo Identificación";
                pos++;
            }

            if (request.BarrioProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Barrio Procedencia";
                pos++;
            }

            if (request.EstadoNNA)
            {
                worksheet.Cell(currentRow, pos).Value = "Estado NNA";
                pos++;
            }

            if (request.NumeroIdentificacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Numero Identificación";
                pos++;
            }

            if (request.AreaProcedencia)
            {
                worksheet.Cell(currentRow, pos).Value = "Area Procedencia";
                pos++;
            }

            if (request.RegimenAfiliacion)
            {
                worksheet.Cell(currentRow, pos).Value = "Regimen Afiliacion";
                pos++;
            }

            if (request.EAPB)
            {
                worksheet.Cell(currentRow, pos).Value = "EAPB";
                pos++;
            }

            foreach (var item in reporte)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = item.FechaNotificacion;
                worksheet.Cell(currentRow, 2).Value = item.NombreNNA;
                worksheet.Cell(currentRow, 3).Value = item.Edad;
                worksheet.Cell(currentRow, 4).Value = item.Sexo;
                worksheet.Cell(currentRow, 5).Value = item.TiempoTranscurrido;
                worksheet.Cell(currentRow, 6).Value = item.Estado;
                pos = 7;
                if (request.CategoriaAlerta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.CategoriaAlerta;
                    pos++;
                }
                if (request.FechaEnvioRespuesta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.FechaEnvioRespuesta;
                    pos++;
                }
                if (request.DepartamentoProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DepartamentoProcedencia;
                    pos++;
                }
                if (request.DireccionProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DireccionProcedencia;
                    pos++;
                }
                if (request.SubcategoriaAlerta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.SubcategoriaAlerta;
                    pos++;
                }
                if (request.Respuesta)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Respuesta;
                    pos++;
                }
                if (request.MunicipioProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.MunicipioProcedencia;
                    pos++;
                }
                if (request.DepartamentoActual)
                {
                    worksheet.Cell(currentRow, pos).Value = item.DepartamentoActual;
                    pos++;
                }
                if (request.Observaciones)
                {
                    worksheet.Cell(currentRow, pos).Value = item.Observaciones;
                    pos++;
                }
                if (request.TipoIdentificacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.TipoIdentificacion;
                    pos++;
                }
                if (request.BarrioProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.BarrioProcedencia;
                    pos++;
                }
                if (request.EstadoNNA)
                {
                    worksheet.Cell(currentRow, pos).Value = item.EstadoNNA;
                    pos++;
                }
                if (request.NumeroIdentificacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.NumeroIdentificacion;
                    pos++;
                }
                if (request.AreaProcedencia)
                {
                    worksheet.Cell(currentRow, pos).Value = item.AreaProcedencia;
                    pos++;
                }
                if (request.RegimenAfiliacion)
                {
                    worksheet.Cell(currentRow, pos).Value = item.RegimenAfiliacion;
                    pos++;
                }
                if (request.EAPB)
                {
                    worksheet.Cell(currentRow, pos).Value = item.EAPB;
                    pos++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return Convert.ToBase64String(content);
        }

        private static string CalcularEdad(DateTime fechaNacimiento)
        {
            var fechaActual = DateTime.Now;
            var edad = fechaActual.Year - fechaNacimiento.Year;
            if (fechaActual.Month < fechaNacimiento.Month || (fechaActual.Month == fechaNacimiento.Month && fechaActual.Day < fechaNacimiento.Day))
                edad--;
            var meses = fechaActual.Month - fechaNacimiento.Month;
            if (fechaActual.Day < fechaNacimiento.Day)
                meses--;
            if (meses < 0)
                meses += 12;
            var dias = fechaActual.Day - fechaNacimiento.Day;
            if (dias < 0)
                dias += DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month);
            return $"{edad} años {meses} meses {dias} días";
        }
    }
}