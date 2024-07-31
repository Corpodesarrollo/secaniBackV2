namespace Core.Request
{
    public class PutSeguimientoActualizacionUsuarioRequest
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public string? ObservacionesSolicitante { get; set; }

    }
}
