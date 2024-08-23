namespace Core.response
{
    public class GetListaCasosResponse
    {
        public long NNAId { get; set; }

        public long SeguimientoId { get; set; }
        public DateTime FechaNotificacionSIVIGILA { get; set; }
        public string Nombre { get; set; }
        public string ObservacionesSolicitante { get; set; }
        public string EstadoAlertasIds { get; set; }

        public int EstadoSeguimientoId { get; set; }

        public int EAPBId { get; set; }
    }
}