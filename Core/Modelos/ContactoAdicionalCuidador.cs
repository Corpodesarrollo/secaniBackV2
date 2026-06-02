using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos
{
    // BUG-LZ-089: telefonos/correos adicionales que el cuidador captura en Mi perfil.
    // Tabla simple unificada (Tipo='telefono'|'correo'). Set se reemplaza por completo al guardar.
    // [Table] fuerza el nombre singular; el DbSet "ContactosAdicionalesCuidador" pluralizaba mal.
    [Table("ContactoAdicionalCuidador")]
    public class ContactoAdicionalCuidador : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
