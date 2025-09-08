using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using Canvas = iText.Layout.Canvas;
using Document = iText.Layout.Document;
using PageSize = iText.Kernel.Geom.PageSize;
using Paragraph = iText.Layout.Element.Paragraph;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace Core.Services.PDF
{
    public class PDFService
    {
        public static byte[] PdfToHtml(string htmlContent, PdfConfig config)
        {
            using var pdfStream = new MemoryStream();
            var headerHeight = 85.14f;
            var footerHeight = 85.14f;

            float cm = 28.35f; // 1 cm ≈ 28.35 pt

            var writer = new PdfWriter(pdfStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.LETTER);

            var properties = new ConverterProperties();
            var fontProvider = new DefaultFontProvider(true, true, true);
            properties.SetFontProvider(fontProvider);

            var pageSize = config.Horizontal ? config.PageSize.Rotate() : config.PageSize;
            pdf.SetDefaultPageSize(pageSize);

            document.SetMargins(config.MarginTop + headerHeight, config.MarginRight, config.MarginBottom + footerHeight, config.MarginLeft);

            // Header (tu código existente)
            if (!string.IsNullOrEmpty(config.HeaderHtmlContent))
            {
                pdf.AddEventHandler(PdfDocumentEvent.START_PAGE, new CustomEventHandler(evt =>
                {
                    try
                    {
                        var page = evt.GetPage();
                        var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
                        var headerRect = new Rectangle(
                            config.MarginLeft,
                            pageSize.GetHeight() - config.MarginTop - headerHeight, // Ajuste clave aquí
                            pageSize.GetWidth() - config.MarginLeft - config.MarginRight,
                            headerHeight
                        );

                        // Procesar el HTML del encabezado
                        var processedHeaderHtml = config.HeaderHtmlContent;
                        // Configuración especial para imágenes
                        var headerProperties = new ConverterProperties();
                        headerProperties.SetFontProvider(fontProvider);
                        // Convertir HTML a elementos
                        var headerElements = HtmlConverter.ConvertToElements(processedHeaderHtml, headerProperties);
                        // Crear canvas para el encabezado
                        var headerCanvas = new Canvas(canvas, headerRect);
                        // Añadir elementos al encabezado
                        foreach (var element in headerElements)
                        {
                            if (element is IBlockElement blockElement)
                            {
                                headerCanvas.Add(blockElement);
                            }
                            else if (element is ILeafElement leafElement)
                            {
                                // Manejar elementos inline como spans
                                var paragraph = new Paragraph().Add(leafElement);
                                headerCanvas.Add(paragraph);
                            }
                        }
                        headerCanvas.Close();
                        canvas.Release();
                    }
                    catch (Exception ex)
                    {
                        // Manejo de errores
                        Console.WriteLine($"Error al procesar encabezado: {ex.Message}");
                    }
                }));
            }

            if (!string.IsNullOrEmpty(config.FooterHtmlContent))
            {
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(evt =>
                {
                    try
                    {
                        var page = evt.GetPage();
                        var pageNum = pdf.GetPageNumber(page);
                        var totalPages = pdf.GetNumberOfPages();

                        // 1. Configuración del área del footer                        
                        float footerYPosition = config.MarginBottom;
                        var footerRect = new Rectangle(
                            config.MarginLeft,
                            25,
                            pageSize.GetWidth() - config.MarginLeft - config.MarginRight,
                            footerHeight
                        );

                        // 2. Procesamiento del HTML
                        var processedFooterHtml = config.FooterHtmlContent;

                        // 3. Configuración especial para imágenes
                        var footerProperties = new ConverterProperties();
                        footerProperties.SetFontProvider(fontProvider);

                        // 4. Convertir HTML a elementos
                        var footerElements = HtmlConverter.ConvertToElements(processedFooterHtml, footerProperties);

                        // 5. Crear canvas para el footer
                        var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
                        var footerCanvas = new Canvas(canvas, footerRect);

                        // 6. Añadir elementos al footer
                        foreach (var element in footerElements)
                        {
                            if (element is IBlockElement blockElement)
                            {
                                footerCanvas.Add(blockElement);
                            }
                            else if (element is ILeafElement leafElement)
                            {
                                // Manejar elementos inline como spans
                                var paragraph = new Paragraph().Add(leafElement);
                                footerCanvas.Add(paragraph);
                            }
                        }

                        footerCanvas.Close();
                        canvas.Release();
                    }
                    catch (Exception ex)
                    {
                        // Manejo de errores
                        Console.WriteLine($"Error al procesar footer: {ex.Message}");
                    }
                }));
            }

            if (config.MostrarNumeracion || !string.IsNullOrEmpty(config.FooterHtmlContent))
            {
                pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new CustomEventHandler(evt =>
                {
                    var pageNum = pdf.GetPageNumber(evt.GetPage());
                    var canvas = new Canvas(evt.GetPage(), pageSize);

                    // Numeración
                    if (config.MostrarNumeracion)
                    {
                        canvas.ShowTextAligned($"Pág {pageNum} de {pdf.GetNumberOfPages()}",
                            pageSize.GetWidth() - config.MarginRight,
                            config.MarginBottom - 20,
                            TextAlignment.RIGHT);
                    }

                    canvas.Close();
                }));
            }

            // Convertir el contenido HTML principal
            HtmlConverter.ConvertToDocument(htmlContent, pdf, properties);

            document.Close();
            return pdfStream.ToArray();
        }
    }
}
