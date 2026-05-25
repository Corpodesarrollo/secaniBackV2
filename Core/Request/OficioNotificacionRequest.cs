namespace Core.Request
{
    public class OficioNotificacionRequest
    {
        public long Id { get; set; }
        public string CiudadEnvio { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Membrete { get; set; }
        public long IdEntidad { get; set; }
        public string Ciudad { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public long IdAlertaSeguimiento { get; set; }
        public string Comentario { get; set; }
        public long IdNNA { get; set; }
        public string Cierre { get; set; }
        public string Firma { get; set; }
        public string FirmaJpg { get; set; }
        public string UserName { get; set; }
    }
}
