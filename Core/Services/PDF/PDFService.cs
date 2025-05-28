using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.IO.Image;
using iText.Kernel.Events;
using iText.Kernel.Pdf;
using iText.Layout;
using Document = iText.Layout.Document;
using Paragraph = iText.Layout.Element.Paragraph;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace Core.Services.PDF
{
    public class PDFService
    {
        public static byte[] PdfToHtml(string htmlContent, PdfConfig config)
        {
            using var pdfStream = new MemoryStream();

            var properties = new ConverterProperties();
            var fontProvider = new DefaultFontProvider(false, true, true);
            properties.SetFontProvider(fontProvider);

            var writer = new PdfWriter(pdfStream);
            var pdf = new PdfDocument(writer);

            var pageSize = config.Horizontal ? config.PageSize.Rotate() : config.PageSize;
            pdf.SetDefaultPageSize(pageSize);

            var document = new Document(pdf, pageSize);
            document.SetMargins(config.MarginTop, config.MarginRight, config.MarginBottom, config.MarginLeft);

            // Header
            if (!string.IsNullOrEmpty(config.Encabezado))
            {
                pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, new CustomEventHandler(evt =>
                {
                    var canvas = new Canvas(evt.GetPage(), pageSize);

                    ImageData imageData = ImageDataFactory.Create(config.RutaLogoEncabezado);
                    var logo = new iText.Layout.Element.Image(imageData);

                    // Convertir de cm a pt
                    float width = 4 * 28.35f;
                    float height = 2 * 28.35f;

                    // Posicionar en la esquina superior derecha
                    float x = pageSize.GetWidth() - width - 28.35f; // 1 cm de margen
                    float y = pageSize.GetTop() - height - 28.35f;  // 1 cm de margen

                    logo.ScaleAbsolute(width, height);
                    logo.SetFixedPosition(x, y);

                    canvas.Add(logo);
                    canvas.Close();
                }));
            }

            // Footer con imagen (si se proporciona)
            if (!string.IsNullOrEmpty(config.RutaImagenPieDePagina) && File.Exists(config.RutaImagenPieDePagina))
            {
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(evt =>
                {
                    var canvas = new Canvas(evt.GetPage(), pageSize);

                    ImageData imageData = ImageDataFactory.Create(config.RutaImagenPieDePagina);
                    var image = new iText.Layout.Element.Image(imageData);

                    // Tamaño del pie de página
                    float width = 260;     // ajusta si es necesario
                    float height = 50;     // ajusta si es necesario
                    float x = (pageSize.GetWidth() - width) / 2;
                    float y = config.MarginBottom;

                    image.ScaleAbsolute(width, height);
                    image.SetFixedPosition(x, y);

                    canvas.Add(image);
                    canvas.Close();
                }));
            }
            else if (config.MostrarNumeracion || !string.IsNullOrEmpty(config.PieDePagina))
            {
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(evt =>
                {
                    var pageNum = pdf.GetPageNumber(evt.GetPage());
                    var canvas = new Canvas(evt.GetPage(), pageSize);

                    // Pie de página con texto
                    if (!string.IsNullOrEmpty(config.PieDePagina))
                    {
                        var paragraph = new Paragraph(config.PieDePagina)
                            .SetFontSize(9)
                            .SetTextAlignment(TextAlignment.CENTER);

                        canvas.ShowTextAligned(paragraph,
                            pageSize.GetWidth() / 2,
                            config.MarginBottom,
                            TextAlignment.CENTER);
                    }

                    // Numeración
                    if (config.MostrarNumeracion)
                    {
                        canvas.ShowTextAligned($"Página {pageNum}",
                            pageSize.GetWidth() - config.MarginRight,
                            config.MarginBottom,
                            TextAlignment.RIGHT);
                    }

                    canvas.Close();
                }));
            }

            HtmlConverter.ConvertToDocument(htmlContent, pdf, properties);

            document.Close();
            return pdfStream.ToArray();
        }
    }
}
