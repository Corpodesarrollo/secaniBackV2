namespace Core.Request
{
    public class PutSeguimientoActualizacionUsuarioRequest
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public string? ObservacionesSolicitante { get; set; }
        // HU SECANI-RQ06-HU01: id del coordinador que ejecuta la reasignacion (para trazabilidad).
        public string? CoordinadorId { get; set; }

    }
}
