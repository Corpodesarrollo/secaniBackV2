using iText.Kernel.Events;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;

namespace Core.Services.PDF
{
    public class FooterEventHandler : IEventHandler
    {
        private readonly PdfDocument _pdf;

        public FooterEventHandler(PdfDocument pdf)
        {
            _pdf = pdf;
        }

        public void HandleEvent(Event @event)
        {
            var docEvent = (PdfDocumentEvent)@event;
            var pageNumber = _pdf.GetPageNumber(docEvent.GetPage());
            var totalPages = _pdf.GetNumberOfPages();

            var canvas = new Canvas(docEvent.GetPage(), docEvent.GetPage().GetPageSize());
            canvas.ShowTextAligned($"Página {pageNumber} de {totalPages}",
                docEvent.GetPage().GetPageSize().GetWidth() - 60, 20,
                TextAlignment.RIGHT);
            canvas.Close();
        }
    }
}
