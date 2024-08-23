using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos
{
    [Table("NotificacionesEntidad")]
    public class NotificacionEntidad : BaseEntity
    {
        public string EntidadId { get; set; }
        public string Cierre { get; set; }
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
        public virtual Entidad Entidad { get; set; }
        public virtual AlertaSeguimiento AlertaSeguimiento { get; set; }
        public virtual NNAs NNAs { get; set; }
        public string Membrete { get; set; }

    }
}
