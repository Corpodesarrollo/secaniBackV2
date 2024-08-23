using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Entidad : BaseEntity
    {
        public int TipoEntidadId { get; set; }
        public string Nombre { get; set; }
    }
}
