using Core.Modelos.Common;

namespace Core.Modelos
{
    public class ContactoEntidad : BaseEntity
    {
        public string EntidadId { get; set; }
        public string Nombres { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
        public bool Activo { get; set; }
        public virtual Entidad Entidad { get; set; }
    }
}
