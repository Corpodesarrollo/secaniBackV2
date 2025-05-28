using iText.Kernel.Geom;

namespace Core.Services.PDF
{
    public class PdfConfig
    {
        public PageSize PageSize { get; set; } = PageSize.LETTER;
        public bool Horizontal { get; set; } = false;

        // Márgenes en centímetros
        public float MarginTopCm { get; set; } = 2.0f;
        public float MarginRightCm { get; set; } = 2.0f;
        public float MarginBottomCm { get; set; } = 2.0f;
        public float MarginLeftCm { get; set; } = 2.0f;

        public string Encabezado { get; set; } = "Encabezado del Documento";
        public string RutaLogoEncabezado { get; set; } = "";
        public string PieDePagina { get; set; } = "Pie de Página del Documento";
        public string RutaImagenPieDePagina { get; set; } = "";
        public bool MostrarNumeracion { get; set; } = true;

        // Propiedades auxiliares para conversión a puntos
        public float MarginTop => CmToPt(MarginTopCm);
        public float MarginRight => CmToPt(MarginRightCm);
        public float MarginBottom => CmToPt(MarginBottomCm);
        public float MarginLeft => CmToPt(MarginLeftCm);

        public PdfConfig() { }

        public PdfConfig(PageSize pageSize, bool horizontal, float marginTopCm, float marginRightCm, float marginBottomCm, float marginLeftCm, string encabezado, bool mostrarNumeracion)
        {
            PageSize = pageSize;
            Horizontal = horizontal;
            MarginTopCm = marginTopCm;
            MarginRightCm = marginRightCm;
            MarginBottomCm = marginBottomCm;
            MarginLeftCm = marginLeftCm;
            Encabezado = encabezado;
            MostrarNumeracion = mostrarNumeracion;
        }

        public PdfConfig(float marginTopCm, float marginRightCm, float marginBottomCm, float marginLeftCm)
        {
            MarginTopCm = marginTopCm;
            MarginRightCm = marginRightCm;
            MarginBottomCm = marginBottomCm;
            MarginLeftCm = marginLeftCm;
        }

        private float CmToPt(float cm) => cm * 28.35f;
    }
}
