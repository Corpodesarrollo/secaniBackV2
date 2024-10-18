namespace Core.response
{
    public class GetNotificacionResponse
    {
        public long IdNotificacion {  get; set; }
        public string TextoNotificacion { get; set; }
        public DateTime FechaNotificacion { get; set; }
        public string URLNotificacion { get; set; }
    }
}
