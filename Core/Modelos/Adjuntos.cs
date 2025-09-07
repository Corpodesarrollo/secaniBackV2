using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Adjuntos : BaseEntity
    {
        public string? NombreArchivo { get; set; }
        public string? Url { get; set; }
        public string? Descripcion { get; set; }
        public long? Referencia { get; set; }
        public TipoAdjunto Tipo { get; set; }
    }

    public enum TipoAdjunto
    {
        Respuesta = 1,
        Notificacion = 2,
        RespuestaNotificacion = 3,
    }
}
