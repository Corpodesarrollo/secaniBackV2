namespace Core.Request
{
    public class CrearAlertaSeguimientoRequest
    {
        public int AlertaId { get; set; }
        public string Observaciones { get; set; }
        public string Username { get; set; }
        public int SeguimientoId { get; set; }
        public int EstadoId { get; set; }
    }
}
