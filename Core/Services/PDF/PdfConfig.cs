using iText.Kernel.Geom;

namespace Core.Services.PDF
{
    public class PdfConfig
    {
        public PageSize PageSize { get; set; } = PageSize.LETTER;
        public bool Horizontal { get; set; } = false;

        // Márgenes en centímetros
        public float MarginTop { get; set; } = 2.0f;
        public float MarginRight { get; set; } = 2.0f;
        public float MarginBottom { get; set; } = 2.0f;
        public float MarginLeft { get; set; } = 2.0f;

        public string HeaderHtmlContent { get; set; } = "Encabezado del Documento";
        public string FooterHtmlContent { get; set; } = "Pie de Página del Documento";
        public bool MostrarNumeracion { get; set; } = true;

        public PdfConfig() { }

        public PdfConfig(PageSize pageSize, bool horizontal, float marginTop, float marginRight, float marginBottom, float marginLeft, string encabezado, bool mostrarNumeracion)
        {
            PageSize = pageSize;
            Horizontal = horizontal;
            MarginTop = marginTop;
            MarginRight = marginRight;
            MarginBottom = marginBottom;
            MarginLeft = marginLeft;
            HeaderHtmlContent = HeaderHtmlContent;
            MostrarNumeracion = mostrarNumeracion;
        }

        public PdfConfig(float marginTop, float marginRight, float marginBottom, float marginLeft)
        {
            MarginTop = marginTop;
            MarginRight = marginRight;
            MarginBottom = marginBottom;
            MarginLeft = marginLeft;
        }
    }
}
