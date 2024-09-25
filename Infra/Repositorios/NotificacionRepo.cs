using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Infra.Repositories
{
    public class NotificacionRepo : INotificacionRepo
    {
        private readonly ApplicationDbContext _context;

        public NotificacionRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<GetNotificacionResponse> GetNotificacionUsuario(string AgenteDestinoId)
        {
            List<GetNotificacionResponse> response = (from un in _context.NotificacionesUsuarios
                                                      join uDestino in _context.AspNetUsers on un.AgenteDestinoId equals uDestino.Id
                                                      join uOrigen in _context.AspNetUsers on un.AgenteOrigenId equals uOrigen.Id
                                                      where un.AgenteDestinoId == AgenteDestinoId && !un.IsDeleted
                                                      select new GetNotificacionResponse()
                                                      {
                                                          IdNotificacion = un.Id,
                                                          TextoNotificacion = string.Join("", "El Agente de seguimiento ", uOrigen.FullName ?? string.Empty,
                                                          " le ha asignado el caso No. ", un.SeguimientoId.ToString() ?? "N/A"),
                                                          FechaNotificacion = un.FechaNotificacion,
                                                          URLNotificacion = un.Url == null ? "" : un.Url
                                                      }).ToList();

            return response;
        }

        public int GetNumeroNotificacionUsuario(string AgenteDestinoId)
        {
            List<GetNotificacionResponse> response = (from un in _context.NotificacionesUsuarios
                                                      join uDestino in _context.AspNetUsers on un.AgenteDestinoId equals uDestino.Id
                                                      join uOrigen in _context.AspNetUsers on un.AgenteDestinoId equals uOrigen.Id
                                                      where un.AgenteDestinoId == AgenteDestinoId && !un.IsDeleted
                                                      select new GetNotificacionResponse()
                                                      {
                                                          TextoNotificacion = string.Join("", "El Agente de seguimiento ", uOrigen.FullName,
                                                          " le ha asignado el caso No. ", un.SeguimientoId)
                                                      }).ToList();

            return response.Count;
        }

        public string GenerarOficioNotificacion(OficioNotificacionRequest request)
        {
            NotificacionEntidad? notificacionEntidad = (from ne in _context.NotificacionesEntidad
                                                        where ne.Id == request.Id
                                                        select ne).FirstOrDefault();

            Entidad? entidad = (from ent in _context.Entidades
                                where ent.Id == request.IdEntidad
                                select ent).FirstOrDefault();

            AlertaSeguimiento? alerta = (from als in _context.AlertaSeguimientos
                                         where als.Id == request.IdAlertaSeguimiento
                                         select als).FirstOrDefault();

            NNAs? nna = (from Tnna in _context.NNAs
                         where Tnna.Id == request.IdNNA
                         select Tnna).FirstOrDefault();

            AspNetUsers? user = (from us in _context.AspNetUsers
                                 where us.UserName == request.UserName
                                 select us).FirstOrDefault();

            if (user == null)
            {
                return "El usuario no existe";
            }


            if (entidad == null)
            {
                return "La entidad no existe";
            }

            if (alerta == null)
            {
                return "La alerta no existe";
            }

            if (nna == null)
            {
                return "el NNA no existe";
            }

            if (notificacionEntidad == null)
            {
                notificacionEntidad = new NotificacionEntidad();
            }

            notificacionEntidad.EntidadId = request.IdEntidad;
            notificacionEntidad.Entidad = entidad;
            notificacionEntidad.Ciudad = request.Ciudad;
            notificacionEntidad.AlertaSeguimiento = alerta;
            notificacionEntidad.Asunto = request.Asunto;
            notificacionEntidad.Cierre = request.Cierre;
            notificacionEntidad.CiudadEnvio = request.CiudadEnvio;
            notificacionEntidad.FechaEnvio = request.FechaEnvio;
            notificacionEntidad.Membrete = request.Membrete;
            notificacionEntidad.Ciudad = request.Ciudad;
            notificacionEntidad.Mensaje = request.Mensaje;
            notificacionEntidad.Comentario = request.Comentario;
            notificacionEntidad.NNAs = nna;
            notificacionEntidad.NNAId = nna.Id;
            notificacionEntidad.Firmajpg = request.FirmaJpg;

            if (notificacionEntidad.Id == 0)
            {
                notificacionEntidad.CreatedByUserId = user.Id;
                notificacionEntidad.DateCreated = DateTime.Now;
            }
            else
            {
                notificacionEntidad.UpdatedByUserId = user.Id;
                notificacionEntidad.DateUpdated = DateTime.Now;
            }

            _context.NotificacionesEntidad.Add(notificacionEntidad);
            _context.SaveChanges();


            return "Oficio creado correctamente";
        }

        public void EliminarNotificacion(EliminarNotificacionRequest request)
        {
            NotificacionesUsuario? notificacion = (from ne in _context.NotificacionesUsuarios
                                                   where ne.Id == request.IdNotificacionUsuario
                                                   select ne).FirstOrDefault();

            if (notificacion != null)
            {
                notificacion.IsDeleted = true;
                notificacion.DeletedByUserId = request.IdUsuario;
                notificacion.DateDeleted = DateTime.Now;

                _context.Update(notificacion);
                _context.SaveChanges();
            }
        }

        public async Task<string> EnviarOficioNotificacion(EnviarOficioNotifcacionRequest request)
        {
            string htmlContent = "<html>\r\n\t<head>\r\n\t\t<meta content=\"text/html; charset=UTF-8\" http-equiv=\"content-type\">\r\n\t\t\t<style type=\"text/css\">@import url(https://themes.googleusercontent.com/fonts/css?kit=fpjTOVmNbO4Lz34iLyptLUXza5VhXqVC6o75Eld_V98);.lst-kix_list_2-6>li:before{content:\"\\002022   \"}.lst-kix_list_2-7>li:before{content:\"\\002022   \"}ul.lst-kix_list_1-0{list-style-type:none}.lst-kix_list_2-4>li:before{content:\"\\002022   \"}.lst-kix_list_2-5>li:before{content:\"\\002022   \"}.lst-kix_list_2-8>li:before{content:\"\\002022   \"}ul.lst-kix_list_2-8{list-style-type:none}ul.lst-kix_list_1-3{list-style-type:none}ul.lst-kix_list_2-2{list-style-type:none}.lst-kix_list_1-0>li:before{content:\"\\0025cf   \"}ul.lst-kix_list_1-4{list-style-type:none}ul.lst-kix_list_2-3{list-style-type:none}ul.lst-kix_list_1-1{list-style-type:none}ul.lst-kix_list_2-0{list-style-type:none}ul.lst-kix_list_1-2{list-style-type:none}ul.lst-kix_list_2-1{list-style-type:none}ul.lst-kix_list_1-7{list-style-type:none}ul.lst-kix_list_2-6{list-style-type:none}.lst-kix_list_1-1>li:before{content:\"o  \"}.lst-kix_list_1-2>li:before{content:\"\\0025aa   \"}ul.lst-kix_list_1-8{list-style-type:none}ul.lst-kix_list_2-7{list-style-type:none}ul.lst-kix_list_1-5{list-style-type:none}ul.lst-kix_list_2-4{list-style-type:none}ul.lst-kix_list_1-6{list-style-type:none}ul.lst-kix_list_2-5{list-style-type:none}.lst-kix_list_1-3>li:before{content:\"\\0025cf   \"}.lst-kix_list_1-4>li:before{content:\"o  \"}.lst-kix_list_1-7>li:before{content:\"o  \"}.lst-kix_list_1-5>li:before{content:\"\\0025aa   \"}.lst-kix_list_1-6>li:before{content:\"\\0025cf   \"}li.li-bullet-0:before{margin-left:-17.6pt;white-space:nowrap;display:inline-block;min-width:17.6pt}.lst-kix_list_2-0>li:before{content:\"\\0025cf   \"}.lst-kix_list_2-1>li:before{content:\"\\002022   \"}.lst-kix_list_1-8>li:before{content:\"\\0025aa   \"}.lst-kix_list_2-2>li:before{content:\"\\002022   \"}.lst-kix_list_2-3>li:before{content:\"\\002022   \"}ol{margin:0;padding:0}table td,table th{padding:0}.c27{border-right-style:solid;padding:0pt 5.4pt 0pt 5.4pt;border-bottom-color:#000000;border-top-width:0pt;border-right-width:0pt;border-left-color:#000000;vertical-align:top;border-right-color:#000000;border-left-width:1pt;border-top-style:solid;border-left-style:solid;border-bottom-width:0pt;width:297.4pt;border-top-color:#000000;border-bottom-style:solid}.c13{border-right-style:solid;padding:0pt 5.4pt 0pt 5.4pt;border-bottom-color:#000000;border-top-width:0pt;border-right-width:1pt;border-left-color:#000000;vertical-align:top;border-right-color:#000000;border-left-width:0pt;border-top-style:solid;border-left-style:solid;border-bottom-width:0pt;width:184.3pt;border-top-color:#000000;border-bottom-style:solid}.c6{background-color:#ffffff;color:#333333;font-weight:700;text-decoration:none;vertical-align:baseline;font-size:12pt;font-family:\"Arial\";font-style:normal}.c0{background-color:#ffffff;color:#333333;font-weight:400;text-decoration:none;vertical-align:baseline;font-size:12pt;font-family:\"Arial\";font-style:normal}.c12{-webkit-text-decoration-skip:none;color:#757b80;font-weight:400;text-decoration:underline;text-decoration-skip-ink:none;font-size:9pt;font-family:\"Arial\"}.c25{margin-left:42.7pt;padding-top:0.1pt;padding-left:-0.4pt;padding-bottom:0pt;line-height:1.0;text-align:justify;margin-right:11.3pt}.c19{color:#008000;font-weight:700;text-decoration:none;vertical-align:baseline;font-size:4pt;font-family:\"inherit\";font-style:normal}.c28{color:#757b80;font-weight:700;text-decoration:none;vertical-align:baseline;font-size:9pt;font-family:\"Arial\";font-style:normal}.c16{color:#000000;font-weight:400;text-decoration:none;vertical-align:baseline;font-size:12pt;font-family:\"Calibri\";font-style:normal}.c24{color:#757b80;font-weight:400;text-decoration:none;vertical-align:baseline;font-size:9pt;font-family:\"Arial\";font-style:normal}.c11{-webkit-text-decoration-skip:none;color:#0563c1;font-weight:400;text-decoration:underline;text-decoration-skip-ink:none;font-family:\"Arial\"}.c1{padding-top:0pt;padding-bottom:0pt;line-height:1.0;orphans:2;widows:2;text-align:justify}.c2{padding-top:0pt;padding-bottom:0pt;line-height:1.0;orphans:2;widows:2;text-align:left}.c22{text-decoration:none;vertical-align:baseline;font-size:12pt;font-style:normal}.c8{background-color:#ffffff;font-family:\"Arial\";color:#333333;font-weight:700}.c30{font-size:7.5pt;font-family:\"inherit\";color:#008000;font-weight:700}.c31{border-spacing:0;border-collapse:collapse;margin-right:auto}.c9{text-decoration:none;vertical-align:baseline;font-size:11pt;font-style:normal}.c4{background-color:#ffffff;font-family:\"Arial\";color:#ff0000;font-weight:400}.c26{color:#000000;font-weight:700;font-family:\"Times New Roman\"}.c15{font-family:\"Arial\";color:#333333;font-weight:400}.c21{font-family:\"Arial\";color:#000000;font-weight:400}.c5{font-family:\"Calibri\";color:#242424;font-weight:400}.c14{font-family:\"Arial\";color:#ff0000;font-weight:700}.c29{color:#000000;font-weight:700;font-family:\"Arial\"}.c17{color:inherit;text-decoration:inherit}.c18{padding:0;margin:0}.c20{max-width:441.9pt;padding:70.8pt 85pt 70.8pt 85pt}.c23{height:18.3pt}.c3{height:12pt}.c10{background-color:#ffffff}.c7{margin-left:8.5pt}.title{padding-top:24pt;color:#000000;font-weight:700;font-size:36pt;padding-bottom:6pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}.subtitle{padding-top:18pt;color:#666666;font-size:24pt;padding-bottom:4pt;font-family:\"Georgia\";line-height:1.0;page-break-after:avoid;font-style:italic;orphans:2;widows:2;text-align:left}li{color:#000000;font-size:12pt;font-family:\"Calibri\"}p{margin:0;color:#000000;font-size:12pt;font-family:\"Calibri\"}h1{padding-top:24pt;color:#000000;font-weight:700;font-size:24pt;padding-bottom:6pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}h2{padding-top:18pt;color:#000000;font-weight:700;font-size:18pt;padding-bottom:4pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}h3{padding-top:14pt;color:#000000;font-weight:700;font-size:14pt;padding-bottom:4pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}h4{padding-top:12pt;color:#000000;font-weight:700;font-size:12pt;padding-bottom:2pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}h5{padding-top:11pt;color:#000000;font-weight:700;font-size:11pt;padding-bottom:2pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}h6{padding-top:10pt;color:#000000;font-weight:700;font-size:10pt;padding-bottom:2pt;font-family:\"Calibri\";line-height:1.0;page-break-after:avoid;orphans:2;widows:2;text-align:left}</style>\r\n\t\t</head>\r\n\t\t<body class=\"c10 c20 doc-content\">\r\n\t\t\t<div>\r\n\t\t\t\t<p class=\"c2\">\r\n\t\t\t\t\t<span style=\"overflow: hidden; display: inline-block; margin: 0.00px 0.00px; border: 0.00px solid #000000; transform: rotate(0.00rad) translateZ(0px); -webkit-transform: rotate(0.00rad) translateZ(0px); width: 207.00px; height: 105.00px;\">\r\n\t\t\t\t\t\t<img alt=\"Imagen que contiene nombre de la empresa\r\n\r\nDescripci&oacute;n generada autom&aacute;ticamente\" src=\"Utilities/header.jpg\" style=\"width: 207.00px; height: 105.00px; margin-left: -0.00px; margin-top: -0.00px; transform: rotate(0.00rad) translateZ(0px); -webkit-transform: rotate(0.00rad) translateZ(0px);\" title=\"\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c2 c3\">\r\n\t\t\t\t\t\t<span class=\"c16\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</div>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c15 c10\">{{CiudadEnvio}} {{FechaEnvio}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c0\">{{Membrete}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c0\">{{NombreEntidad}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c0\">Ciudad </span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c8\">Asunto.</span>\r\n\t\t\t\t\t<span class=\"c0\">{{Asunto}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c0\">{{Mensaje}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<ul class=\"c18 lst-kix_list_2-0 start\">\r\n\t\t\t\t\t<li class=\"c25 li-bullet-0\">\r\n\t\t\t\t\t\t<span class=\"c22 c29\">{{Alerta}}</span>\r\n\t\t\t\t\t\t<span class=\"c22 c21\">{{ObservacionAlerta}}</span>\r\n\t\t\t\t\t</li>\r\n\t\t\t\t</ul>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c0\">{{Comentario}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c6\">Nombre:{{NombreNNA}} </span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c6\">Identificaci&oacute;n:{{DocumentoNNA}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c6\">Edad. {{EdadNNA}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c6\">Diagn&oacute;stico:{{DiagnosticoNNA}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c6\">Tel&eacute;fono del acudiente:{{TelefonoAcudienteNNA}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1\">\r\n\t\t\t\t\t<span class=\"c10 c15\">{{Cierre}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c2 c10\">\r\n\t\t\t\t\t<span class=\"c0\">{{Firma}}</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c2 c3 c10\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c2 c3 c10\">\r\n\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t</p>\r\n\t\t\t\t<p class=\"c2 c10\">\r\n\t\t\t\t\t<span class=\"c0\">&nbsp;</span>\r\n\t\t\t\t</p>\r\n\t\t\t\t<a id=\"t.216db21d5c9c489270f6750af11c27c9f468ceea\"/>\r\n\t\t\t\t<a id=\"t.0\"/>\r\n\t\t\t\t<table class=\"c31\">\r\n\t\t\t\t\t<tr class=\"c23\">\r\n\t\t\t\t\t\t<td class=\"c10 c13\" colspan=\"1\" rowspan=\"1\">\r\n\t\t\t\t\t\t\t<p class=\"c2\">\r\n\t\t\t\t\t\t\t\t<span style=\"overflow: hidden; display: inline-block; margin: 0.00px 0.00px; border: 0.00px solid #000000; transform: rotate(0.00rad) translateZ(0px); -webkit-transform: rotate(0.00rad) translateZ(0px); width: 207.00px; height: 105.00px;\">\r\n\t\t\t\t\t\t\t\t\t<img alt=\"Imagen que contiene nombre de la empresa\r\n\r\nDescripci&oacute;n generada autom&aacute;ticamente\" src=\"Utilities/footer.jpg\" style=\"width: 207.00px; height: 105.00px; margin-left: 0.00px; margin-top: 0.00px; transform: rotate(0.00rad) translateZ(0px); -webkit-transform: rotate(0.00rad) translateZ(0px);\" title=\"\"/>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t\t<td class=\"c10 c27\" colspan=\"1\" rowspan=\"1\">\r\n\t\t\t\t\t\t\t\t<p class=\"c2 c7\">\r\n\t\t\t\t\t\t\t\t\t<span class=\"c5\">&nbsp;</span>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t\t<p class=\"c2 c7\">\r\n\t\t\t\t\t\t\t\t\t<span class=\"c28\">Estrategia De Seguimiento Nacional de C&aacute;ncer</span>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t\t<p class=\"c2 c7\">\r\n\t\t\t\t\t\t\t\t\t<span class=\"c24\">Centro de Contacto al Ciudadano</span>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t\t<p class=\"c2 c7\">\r\n\t\t\t\t\t\t\t\t\t<span class=\"c24\">Tel&eacute;fono: 601 330 5043</span>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t\t<p class=\"c2 c7\">\r\n\t\t\t\t\t\t\t\t\t<span class=\"c12\">\r\n\t\t\t\t\t\t\t\t\t\t<a class=\"c17\" href=\"https://www.google.com/url?q=http://www.minsalud.gov.co/&amp;sa=D&amp;source=editors&amp;ust=1721344138250935&amp;usg=AOvVaw2V56Er3RR38XSn8yZJykQG\">www.minsalud.gov.co</a>\r\n\t\t\t\t\t\t\t\t\t</span>\r\n\t\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t</tr>\r\n\t\t\t\t\t</table>\r\n\t\t\t\t\t<p class=\"c2 c3 c10\">\r\n\t\t\t\t\t\t<span class=\"c19\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c2 c10\">\r\n\t\t\t\t\t\t<span class=\"c30\">Antes de imprimir este mensaje piense bien si es necesario hacerlo.</span>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t\t<p class=\"c1 c3\">\r\n\t\t\t\t\t\t<span class=\"c0\"/>\r\n\t\t\t\t\t</p>\r\n\t\t\t\t</body>\r\n\t\t\t</html>";

            NotificacionEntidadPlantilla? notificacionEntidadPlantilla = (from ne in _context.NotificacionesEntidad
                                                                          join ent in _context.Entidades on ne.EntidadId equals ent.Id
                                                                          join alseg in _context.AlertaSeguimientos on ne.AlertaSeguimientoId equals alseg.Id
                                                                          join al in _context.Alertas on alseg.AlertaId equals al.Id
                                                                          join nna in _context.NNAs on ne.NNAId equals nna.Id
                                                                          where ne.Id == request.IdNotificacionEntidad
                                                                          select new NotificacionEntidadPlantilla()
                                                                          {
                                                                              Asunto = ne.Asunto,
                                                                              Cierre = ne.Cierre,
                                                                              CiudadEnvio = ne.CiudadEnvio,
                                                                              ComentarioNotificacion = ne.Comentario,
                                                                              DescripcionAlerta = al.Descripcion,
                                                                              DiagnosticoNNA = nna.DiagnosticoId,
                                                                              DocumentoNNA = nna.NumeroIdentificacion,
                                                                              FechaNacimientoNNA = nna.FechaNacimiento,
                                                                              FechaEnvio = ne.FechaEnvio,
                                                                              Firma = ne.Firmajpg,
                                                                              Membrete = ne.Membrete,
                                                                              Mensaje = ne.Mensaje,
                                                                              NombreEntidad = ent.Nombre,
                                                                              ObservacionAlerta = alseg.Observaciones,
                                                                              PrimerApellidoNNA = nna.PrimerApellido,
                                                                              PrimerNombreNNA = nna.PrimerNombre,
                                                                              SegundoApellidoNNA = nna.SegundoApellido,
                                                                              SegundoNombreNNA = nna.SegundoNombre,
                                                                              TelefonoAcudienteNNA = nna.CuidadorTelefono
                                                                          }).FirstOrDefault();

            if (notificacionEntidadPlantilla != null)
            {
                if (notificacionEntidadPlantilla.FechaNacimientoNNA != null)
                {
                    DateTime fechaNacimiento = notificacionEntidadPlantilla.FechaNacimientoNNA.Value;
                    notificacionEntidadPlantilla.EdadNNA = DateTime.Today.Year - fechaNacimiento.Year -
                    (DateTime.Today.DayOfYear < fechaNacimiento.DayOfYear ? 1 : 0);

                }


                htmlContent = htmlContent.Replace("{{CiudadEnvio}}", notificacionEntidadPlantilla.CiudadEnvio)
                                         .Replace("{{FechaEnvio}}", notificacionEntidadPlantilla.FechaEnvio.ToString("yyyy/MM/dd"))
                                         .Replace("{{Membrete}}", notificacionEntidadPlantilla.Membrete)
                                         .Replace("{{NombreEntidad}}", notificacionEntidadPlantilla.NombreEntidad)
                                         .Replace("{{Asunto}}", notificacionEntidadPlantilla.Asunto)
                                         .Replace("{{Mensaje}}", notificacionEntidadPlantilla.Mensaje)
                                         .Replace("{{Alerta}}", notificacionEntidadPlantilla.DescripcionAlerta)
                                         .Replace("{{ObservacionAlerta}}", notificacionEntidadPlantilla.ObservacionAlerta)
                                         .Replace("{{Comentario}}", notificacionEntidadPlantilla.ComentarioNotificacion)
                                         .Replace("{{NombreNNA}}", string.Concat(notificacionEntidadPlantilla.PrimerNombreNNA, " ", notificacionEntidadPlantilla.SegundoNombreNNA, " ",
                                         notificacionEntidadPlantilla.PrimerApellidoNNA, " ", notificacionEntidadPlantilla.SegundoApellidoNNA))
                                         .Replace("{{DocumentoNNA}}", notificacionEntidadPlantilla.DocumentoNNA)
                                         .Replace("{{EdadNNA}}", Convert.ToString(notificacionEntidadPlantilla.EdadNNA))
                                         .Replace("{{DiagnosticoNNA}}", notificacionEntidadPlantilla.DiagnosticoNNA.ToString())
                                         .Replace("{{TelefonoAcudienteNNA}}", notificacionEntidadPlantilla.TelefonoAcudienteNNA)
                                         .Replace("{{Cierre}}", notificacionEntidadPlantilla.Cierre)
                                         .Replace("{{Firma}}", notificacionEntidadPlantilla.Firma);

                await new BrowserFetcher().DownloadAsync();

                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                await using var page = await browser.NewPageAsync();
                await page.SetContentAsync(htmlContent);

                var pdfStream = await page.PdfStreamAsync(new PdfOptions
                {
                    Format = PaperFormat.Letter,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "1cm",
                        Right = "1cm",
                        Bottom = "1cm",
                        Left = "1cm"
                    }
                });

                await browser.CloseAsync();

                pdfStream.Position = 0;
                var pdfBytes = new MemoryStream();
                await pdfStream.CopyToAsync(pdfBytes);
                pdfBytes.Position = 0;



                List<EmailConfiguration> emailConfigurations = _context.EmailConfigurations.ToList();

                if (emailConfigurations.Count > 0)
                {
                    EmailConfiguration emailConfiguration = emailConfigurations[0];
                    SmtpClient clienteSmtp = new(emailConfiguration.SmtpServer)
                    {
                        Port = 587, // Puerto SMTP
                        Credentials = new NetworkCredential(emailConfiguration.UserName, emailConfiguration.Password),
                        EnableSsl = emailConfiguration.EnableSsl, // Habilitar SSL
                    };

                    // Creación del mensaje de correo
                    MailMessage mensaje = new()
                    {
                        From = new MailAddress(emailConfiguration.UserName),
                        Subject = request.Asunto,
                        Body = request.Comentario,
                        IsBodyHtml = false, // Cambiar a true si el cuerpo del correo es HTML
                    };

                    // Agregar destinatario
                    if (request.Para.Length > 0)
                        foreach (var item in request.Para)
                            mensaje.To.Add(item);

                    if (request.ConCopia.Length > 0)
                        foreach (var item in request.ConCopia)
                            mensaje.CC.Add(item);

                    // Crear y agregar el archivo adjunto desde byte[]
                    using var ms = new MemoryStream(pdfBytes.ToArray());
                    Attachment adjunto = new(ms, "OficioNotificacion.pdf", MediaTypeNames.Application.Octet);
                    mensaje.Attachments.Add(adjunto);

                    // Enviar el correo
                    clienteSmtp.Send(mensaje);
                }
                return "Oficio de notificacion enviado correctamente";
            }
            else
            {
                return "oficio de notificacion no encontrado";
            }
        }

        public VerOficioNotificacionResponse VerOficioNotificacion(VerOficioNotificacionRequest request)
        {
            NotificacionEntidad? notificacion = (from ne in _context.NotificacionesEntidad
                                                 where ne.Id == request.IdNotificacion
                                                 select ne).FirstOrDefault();

            VerOficioNotificacionResponse response;

            if (notificacion != null)
            {
                response = new VerOficioNotificacionResponse()
                {
                    FechaNotificacion = notificacion.FechaEnvio,
                    Para = notificacion.EmailPara,
                    Firma = notificacion.Firmajpg,
                    ConCopia = notificacion.EmailCC,
                    Mensaje = notificacion.Mensaje,
                    Asunto = notificacion.Asunto,
                };
            }
            else
            {
                response = new VerOficioNotificacionResponse();
            }
            return response;
        }


        public List<GetNotificacionesEntidadResponse> RepoNotificacionEntidadCasos(long entidadId, int alertaSeguimientoId, int nnaId)
        {
            List<GetNotificacionesEntidadResponse> notificacionEntidad = (from ne in _context.NotificacionesEntidad
                                                                          where ne.EntidadId == entidadId
                                                                          && ne.AlertaSeguimientoId == alertaSeguimientoId
                                                                          && ne.NNAId == nnaId

                                                                          select new GetNotificacionesEntidadResponse()
                                                                          {
                                                                              EntidadId = ne.EntidadId,

                                                                              CiudadEnvio = ne.CiudadEnvio,
                                                                              FechaEnvio = ne.FechaEnvio,
                                                                              AlertaSeguimientoId = ne.AlertaSeguimientoId,
                                                                              NNAId = ne.NNAId,
                                                                              Ciudad = ne.Ciudad,
                                                                              EmailConfigurationId = ne.EmailConfigurationId,
                                                                              EmailPara = ne.EmailPara,
                                                                              EmailCC = ne.EmailCC,
                                                                              PlantillaId = ne.PlantillaId,
                                                                              Asunto = ne.Asunto,
                                                                              Mensaje = ne.Mensaje,
                                                                              EnlaceParaRecibirRespuestas = ne.EnlaceParaRecibirRespuestas,
                                                                              Comentario = ne.Comentario,
                                                                              Firmajpg = ne.Firmajpg,
                                                                              ArchivoAdjunto = ne.ArchivoAdjunto
                                                                          }
                                                                          ).ToList();

            return notificacionEntidad;
        }


        public List<GetListaCasosResponse> RepoListaCasosNotificacion(int eapbId, int epsId)
        {
            List<GetListaCasosResponse> listaCasos = (from n in _context.NNAs
                                                      join s in _context.Seguimientos on n.Id equals s.NNAId
                                                      join a in _context.AlertaSeguimientos on s.Id equals a.SeguimientoId

                                                      where n.EAPBId == eapbId || n.EPSId == epsId
                                                      group new { n, s, a } by new
                                                      {
                                                          NNAId = n.Id,
                                                          n.FechaNotificacionSIVIGILA,
                                                          n.EAPBId,
                                                          Nombre = n.PrimerNombre + " " + n.SegundoNombre + " " + n.PrimerApellido + " " + n.SegundoApellido
                                                      } into g
                                                      select new
                                                      {
                                                          g.Key.NNAId,
                                                          g.Key.FechaNotificacionSIVIGILA,
                                                          g.Key.Nombre,
                                                          g.Key.EAPBId,
                                                          Seguimientos = g.Select(x => x.s).OrderByDescending(sg => sg.FechaSeguimiento).ToList(),  // Convertir a lista
                                                          Estados = g.Select(x => x.a.EstadoId).Distinct()
                                                      }).AsEnumerable() // Cambiar a evaluación en el cliente
              .Select(g => new GetListaCasosResponse
              {
                  NNAId = g.NNAId,
                  SeguimientoId = g.Seguimientos.FirstOrDefault().Id,
                  FechaNotificacionSIVIGILA = g.FechaNotificacionSIVIGILA,
                  Nombre = g.Nombre,
                  EAPBId = g.EAPBId,
                  ObservacionesSolicitante = g.Seguimientos.FirstOrDefault()?.ObservacionesSolicitante,
                  EstadoAlertasIds = string.Join(",", g.Estados),
                  EstadoSeguimientoId = g.Seguimientos.FirstOrDefault().EstadoId

              }).ToList();
            return listaCasos;
        }

        public List<NotificacionResponse> GetNotificacionAlerta(long AlertaId)
        {
            List<NotificacionResponse> response = (from un in _context.Notificacions
                                                   join ent in _context.Entidades on un.EntidadId equals ent.Id
                                                   where un.AlertaSeguimientoId == AlertaId && !un.IsDeleted
                                                   select new NotificacionResponse()
                                                   {
                                                       EntidadNotificada = ent.Nombre,
                                                       FechaNotificacion = un.FechaNotificacion,
                                                       FechaRespuesta = un.FechaRespuesta,
                                                       Respuesta = un.RespuestaEntidad
                                                   }).ToList();

            return response;
        }
    }
}
