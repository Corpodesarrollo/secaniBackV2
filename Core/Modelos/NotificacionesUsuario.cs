using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos
{
    [Table("NotificacionesUsuario")]
    public class NotificacionesUsuario : BaseEntity
    {
        //Enum
        public int TipoNotificacionId { get; set; }
        //Nro de Caso
        public long SeguimientoId { get; set; }
        //UsuarioId del agente origen
        public string AgenteOrigenId { get; set; }
        // UsuarioId del agente destino
        public string AgenteDestinoId { get; set; }
        public DateTime FechaNotificacion { get; set; }
        public string Accion { get; set; }
        public bool CoordinadorOAdministrador { get; set; }
        public string? Url {  get; set; }
    }
}
