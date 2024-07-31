namespace Core.Request
{
    public class PutSeguimientoActualizacionFechaRequest
    {
        public int Id { get; set; }
        public DateTime FechaSeguimiento { get; set; }
        public string? ObservacionesSolicitante { get; set; }

    }
}
