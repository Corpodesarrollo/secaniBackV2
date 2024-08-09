using Core.Modelos.Common;

namespace Core.Modelos
{
    public class ContactoNNA : BaseEntity
    {
        public long NNAId { get; set; }
        public string Nombres { get; set; }
        public int ParentescoId { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
        public string TelefnosInactivos { get; set; }
        public bool Cuidador { get; set; } = false;
    }
}
