using Core.Modelos.Common;

namespace Core.Modelos
{
    public class UsuarioAsignado : BaseEntity
    {
        public string UsuarioId { get; set; }
        public long SeguimientoId { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string Observaciones { get; set; }

        public Boolean Activo { get; set; }
    }
}
