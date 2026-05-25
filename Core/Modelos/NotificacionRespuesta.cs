namespace Core.Modelos
{
    public class NotificacionRespuesta
    {
        public long Id { get; set; }
        public string? Entidad { get; set; }
        public string? NombreFuncionario { get; set; }
        public string? Cargo { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Respuesta { get; set; }
        public long IdAdjunto { get; set; }
    }
}
