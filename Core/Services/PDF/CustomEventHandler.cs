using iText.Kernel.Events;

namespace Core.Services.PDF
{
    class CustomEventHandler : IEventHandler
    {
        private readonly Action<PdfDocumentEvent> _action;

        public CustomEventHandler(Action<PdfDocumentEvent> action)
        {
            _action = action;
        }

        public void HandleEvent(Event @event)
        {
            _action((PdfDocumentEvent)@event);
        }
    }
}
