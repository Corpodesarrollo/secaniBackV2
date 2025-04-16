using Core.Modelos.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Modelos
{
    public class UsuarioAsignado : BaseEntity
    {
        public string UsuarioId { get; set; }
        public long SeguimientoId { get; set; }

        [NotMapped]
        public string? NombreUsuario { get; set; }

        [NotMapped]
        public string? NombreNNA { get; set; }

        [NotMapped]
        public string? DocumentoNNA { get; set; }

        public DateTime? FechaAsignacion { get; set; }
        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        [NotMapped]
        public string? Criterio { get; set; }
    }
}
