using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Identity;
using Core.Request;
using Core.Services.StorageService;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO.Compression;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Infra.Repositories
{
    public class AlertaRepo : IAlertaRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly IStorageService _storageService;

        public AlertaRepo(ApplicationDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public string CrearAlertaSeguimiento(CrearAlertaSeguimientoRequest request)
        {
            try
            {
                ApplicationUser? user = (from users in _context.Users
                                         where users.UserName == request.Username
                                         select users).FirstOrDefault();

                if (user == null)
                {
                    return "Usuario no encontrado";
                }
                else
                {
                    var alertaSeguimiento = new AlertaSeguimiento()
                    {
                        CreatedByUserId = user.Id,
                        DateCreated = new DateTime(),
                        EstadoId = request.EstadoId,
                        AlertaId = request.AlertaId,
                        Observaciones = request.Observaciones,
                        SeguimientoId = request.SeguimientoId,
                        UltimaFechaSeguimiento = new DateTime(),
                    };

                    _context.AlertaSeguimientos.Add(alertaSeguimiento);
                    _context.SaveChanges();

                    return "Alerta creada exitosamente";
                }
            }
            catch (Exception)
            {
                return "Se presento un problema al crear la alerta";
            }
        }

        public string GestionarAlerta(GestionarAlertaRequest request)
        {
            try
            {
                ApplicationUser? user = (from users in _context.Users
                                         where users.UserName == request.UserName
                                         select users).FirstOrDefault();

                if (user == null)
                {
                    return "Usuario no encontrado";
                }
                else
                {
                    AlertaSeguimiento? seguimiento = (from aSeguimiento in _context.AlertaSeguimientos
                                                      where aSeguimiento.Id == request.IdSeguimiento
                                                      select aSeguimiento).FirstOrDefault();

                    if (seguimiento == null)
                    {
                        AlertaSeguimiento alertaSeguimiento = new()
                        {
                            AlertaId = request.IdAlerta,
                            CreatedByUserId = user.Id,
                            DateCreated = new DateTime(),
                            EstadoId = request.IdEstado,
                            Observaciones = request.Observacion,
                            UltimaFechaSeguimiento = new DateTime()
                        };
                        _context.AlertaSeguimientos.Add(alertaSeguimiento);
                        _context.SaveChanges();

                        return "Seguimiento registrado exitosamente";
                    }
                    else
                    {
                        seguimiento.Observaciones = request.Observacion;
                        seguimiento.EstadoId = request.IdEstado;
                        seguimiento.DateUpdated = new DateTime();
                        seguimiento.UltimaFechaSeguimiento = new DateTime();
                        seguimiento.UpdatedByUserId = user.Id;

                        _context.Update(seguimiento);
                        _context.SaveChanges();

                        return "Seguimiento actualizado exitosamente";
                    }

                }
            }
            catch (Exception)
            {
                return "Se presento un problema al gestionar el seguimiento";
            }
        }

        public List<AlertaSeguimiento> ConsultarAlertaSeguimiento(ConsultarAlertasRequest request)
        {
            List<AlertaSeguimiento> response = (from aseg in _context.AlertaSeguimientos
                                                where aseg.SeguimientoId == request.IdSeguimiento
                                                select aseg).ToList();

            return response;
        }

        public async Task<AlertaSeguimientoDto[]> ConsultarAlertasUltimoSeguimiento(int idNNA)
        {
            var query = from s in _context.Seguimientos
                        join n in _context.NNAs on s.NNAId equals n.Id
                        where n.Id == idNNA
                        group s by s.NNAId into g
                        select new { id = g.Max(x => x.Id) };


            var response = await (from q in query
                                  join ase in _context.AlertaSeguimientos on q.id equals ase.SeguimientoId
                                  join a in _context.Alertas on ase.AlertaId equals a.Id
                                  join ea in _context.TPEstadoAlerta on ase.EstadoId equals ea.Id
                                  join sca in _context.TPSubCategoriaAlerta on a.SubcategoriaId equals sca.Id
                                  where ea.Id == 1 || ea.Id == 3
                                  select new AlertaSeguimientoDto
                                  {
                                      Id = ea.Id,
                                      IdAlerta = sca.Id,
                                      Nombre = sca.CategoriaAlertaId + "." + sca.Indicador
                                  }).ToArrayAsync();

            return response;
        }

        public List<AlertaSeguimiento> ConsultarAlertaEstados(ConsultarAlertasEstadosRequest request)
        {
            List<AlertaSeguimiento> alertasSeguimiento = _context.AlertaSeguimientos
                              .Where(u => request.estados.Contains(u.EstadoId))
                              .ToList();

            return alertasSeguimiento;
        }

        public async Task<(byte[], string)> Exportar(int idAlerta)
        {
            try
            {
                string nombreArchivoRespuestaNotificacion = string.Empty;
                string nombreArchivoAdjunto = string.Empty;
                string nombreArchivo = string.Empty;

                var nna = await (from asg in _context.AlertaSeguimientos
                                 join s in _context.Seguimientos on asg.SeguimientoId equals s.Id
                                 join n in _context.NNAs on s.NNAId equals n.Id
                                 join eapb in _context.TPEAPB on n.EAPBId equals eapb.Id into eapbGroup
                                 from eapb in eapbGroup.DefaultIfEmpty()
                                 where asg.AlertaId == idAlerta
                                 select new
                                 {
                                     n.Id,
                                     n.PrimerNombre,
                                     n.SegundoNombre,
                                     n.PrimerApellido,
                                     n.SegundoApellido,
                                     n.EAPBId,
                                     EAPBNombre = eapb.Nombre,
                                     s.FechaSeguimiento
                                 }).FirstOrDefaultAsync();

                nombreArchivo = $"{nna.Id} - {nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}.zip";

                using var zipStream = new MemoryStream();
                using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Cargar adjunto respuesta
                    var adjuntos = await _context.Adjuntos.FirstOrDefaultAsync(x => x.Referencia == idAlerta && x.Tipo == TipoAdjunto.Respuesta);
                    if (adjuntos != null)
                        nombreArchivoAdjunto = adjuntos.NombreArchivo ?? "";

                    if (!string.IsNullOrEmpty(nombreArchivoAdjunto))
                    {
                        try
                        {
                            var respuestaAdjunto = await _storageService.DownloadFileAsync(nombreArchivoAdjunto);
                            if (respuestaAdjunto != null && respuestaAdjunto.Length > 0)
                            {
                                var entry = zip.CreateEntry(nombreArchivoAdjunto, System.IO.Compression.CompressionLevel.Fastest);
                                using (var entryStream = entry.Open())
                                {
                                    await entryStream.WriteAsync(respuestaAdjunto, 0, respuestaAdjunto.Length);
                                }
                            }
                        }
                        catch { /* Archivo no encontrado en storage */ }
                    }

                    // Cargar Notificacion 
                    var notificacion = await (from ne in _context.NotificacionesEntidad
                                              join asi in _context.AlertaSeguimientos on ne.AlertaSeguimientoId equals asi.Id
                                              where asi.AlertaId == idAlerta
                                              select ne).FirstOrDefaultAsync();

                    List<string>? nombresArchivos = new();
                    if (notificacion != null)
                    {
                        var adjuntoNotificacion = await _context.Adjuntos.Where(x => x.Referencia == notificacion.Id && x.Tipo == TipoAdjunto.Notificacion).ToListAsync();
                        if (adjuntoNotificacion.Count != 0)
                        {
                            foreach (var item in adjuntoNotificacion)
                            {
                                var nombreArchivoNotificacion = item.NombreArchivo ?? "";
                                nombresArchivos.Add(nombreArchivoNotificacion);
                                if (!string.IsNullOrEmpty(nombreArchivoNotificacion))
                                {
                                    try
                                    {
                                        var notificacionAdjunto = await _storageService.DownloadFileAsync(nombreArchivoNotificacion);
                                        if (notificacionAdjunto != null && notificacionAdjunto.Length > 0)
                                        {
                                            var notificacionEntry = zip.CreateEntry(nombreArchivoNotificacion, System.IO.Compression.CompressionLevel.Fastest);
                                            using (var notificacionEntryStream = notificacionEntry.Open())
                                            {
                                                await notificacionEntryStream.WriteAsync(notificacionAdjunto, 0, notificacionAdjunto.Length);
                                            }
                                        }
                                    }
                                    catch { /* Archivo no encontrado en storage */ }
                                }
                            }
                        }
                    }

                    // Cargar Respuesta Notificacion
                    var respuestaNotificacion = await (from ne in _context.NotificacionesEntidad
                                                       join asi in _context.AlertaSeguimientos on ne.AlertaSeguimientoId equals asi.Id
                                                       where asi.AlertaId == idAlerta
                                                       select ne).FirstOrDefaultAsync();
                    if (respuestaNotificacion != null)
                    {
                        var adjuntoRespuestaNotificacion = await _context.Adjuntos.FirstOrDefaultAsync(x => x.Referencia == respuestaNotificacion.Id && x.Tipo == TipoAdjunto.RespuestaNotificacion);
                        if (adjuntoRespuestaNotificacion != null)
                        {
                            nombreArchivoRespuestaNotificacion = adjuntoRespuestaNotificacion.NombreArchivo ?? "";
                            if (!string.IsNullOrEmpty(nombreArchivoRespuestaNotificacion))
                            {
                                try
                                {
                                    var respuestaNotificacionAdjunto = await _storageService.DownloadFileAsync(nombreArchivoRespuestaNotificacion);
                                    if (respuestaNotificacionAdjunto != null && respuestaNotificacionAdjunto.Length > 0)
                                    {
                                        var respuestaEntry = zip.CreateEntry(nombreArchivoRespuestaNotificacion, System.IO.Compression.CompressionLevel.Fastest);
                                        using (var respuestaEntryStream = respuestaEntry.Open())
                                        {
                                            await respuestaEntryStream.WriteAsync(respuestaNotificacionAdjunto, 0, respuestaNotificacionAdjunto.Length);
                                        }
                                    }
                                }
                                catch { /* Archivo no encontrado en storage */ }
                            }
                        }
                    }

                    // Cargar Excel
                    var excel = await GenerarExcel(nombreArchivoAdjunto, nombresArchivos, nombreArchivoRespuestaNotificacion, nna.EAPBNombre, nna.FechaSeguimiento);
                    if (excel != null)
                    {
                        var excelEntry = zip.CreateEntry($"{nna.Id} - {nna.PrimerNombre} {nna.SegundoNombre} {nna.PrimerApellido} {nna.SegundoApellido}.xlsx", System.IO.Compression.CompressionLevel.Fastest);
                        using (var excelEntryStream = excelEntry.Open()) // ← Aquí está la corrección
                        {
                            await excelEntryStream.WriteAsync(excel, 0, excel.Length);
                        }
                    }
                }

                return (zipStream.ToArray(), nombreArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception("Se presento un problema al exportar la alerta");
            }
        }

        private static async Task<byte[]?> GenerarExcel(
            string nombreArchivoRespuesta,
            List<string>? nombreArchivoNotificacion,
            string nombreArchivoRespuestaNotificacion,
            string eapb, DateTime? fechaNotificacion)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Alertas");

            // 👉 Encabezados (ajusta según las columnas de tu entidad Alerta)
            ws.Cells[1, 1].Value = "Nombre EAPB";
            ws.Cells[1, 2].Value = "Fecha Notificación";
            ws.Cells[1, 3].Value = "Respuesta";
            var colIndex = 4;
            foreach (var nombre in nombreArchivoNotificacion)
            {
                ws.Cells[1, colIndex].Value = nombre.Contains("Adj") ? "NotificacionAdjunto" : "Notificacion";
                colIndex++;
            }
            ws.Cells[1, colIndex].Value = "RespuestaNotificacion";

            var cntCol = (nombreArchivoNotificacion?.Count ?? 0) + 4;
            using (var range = ws.Cells[1, 1, 1, cntCol])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // 👉 Rellenar datos
            ws.Cells[2, 1].Value = eapb;
            ws.Cells[2, 2].Value = fechaNotificacion?.ToString("yyyy-MM-dd hh:mm tt");

            //cargar url respuesta
            ws.Cells[2, 3].Hyperlink = new Uri($"https://sigoat.blob.core.windows.net/attachments/{nombreArchivoRespuesta}");
            ws.Cells[2, 3].Value = nombreArchivoRespuesta;
            ws.Cells[2, 3].Style.Font.UnderLine = true;
            ws.Cells[2, 3].Style.Font.Color.SetColor(System.Drawing.Color.Blue);

            // Cargar Notificacion
            colIndex = 4;
            foreach (var nombre in nombreArchivoNotificacion)
            {
                ws.Cells[2, colIndex].Hyperlink = new Uri($"https://sigoat.blob.core.windows.net/attachments/{nombre}");
                ws.Cells[2, colIndex].Value = nombre;
                ws.Cells[2, colIndex].Style.Font.UnderLine = true;
                ws.Cells[2, colIndex].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                colIndex++;
            }

            // Cargar Respuesta Notificacion
            ws.Cells[2, colIndex].Hyperlink = new Uri($"https://sigoat.blob.core.windows.net/attachments/{nombreArchivoRespuestaNotificacion}");
            ws.Cells[2, colIndex].Value = nombreArchivoRespuestaNotificacion;
            ws.Cells[2, colIndex].Style.Font.UnderLine = true;
            ws.Cells[2, colIndex].Style.Font.Color.SetColor(System.Drawing.Color.Blue);

            ws.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }
}
