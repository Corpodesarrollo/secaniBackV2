using iText.Kernel.Events;
using iText.Layout;
using iText.Layout.Properties;

namespace Core.Services.PDF
{
    public class HeaderEventHandler : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            var docEvent = (PdfDocumentEvent)@event;
            var canvas = new Canvas(docEvent.GetPage(), docEvent.GetPage().GetPageSize());
            canvas.ShowTextAligned("Encabezado personalizado", 36, docEvent.GetPage().GetPageSize().GetTop() - 20,
                TextAlignment.LEFT);
            canvas.Close();
        }
    }
}
