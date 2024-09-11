namespace Core.response
{
    public class GetNotificacionesEntidadResponse
    {
        public string EntidadId { get; set; }
        public string CiudadEnvio { get; set; }
        public DateTime FechaEnvio { get; set; }
        public long AlertaSeguimientoId { get; set; }
        public long NNAId { get; set; }
        public string Ciudad { get; set; }
        public long EmailConfigurationId { get; set; }
        public string EmailPara { get; set; }
        public string EmailCC { get; set; }
        public long PlantillaId { get; set; }
        public string Asunto { get; set; }
        public string Mensaje { get; set; }
        public string EnlaceParaRecibirRespuestas { get; set; }
        public string Comentario { get; set; }
        public string Firmajpg { get; set; }
        public string ArchivoAdjunto { get; set; }
    }
}



