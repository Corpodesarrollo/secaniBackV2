using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Ausencias: BaseEntity
    {
        public string? UsuarioId { get; set; }
        public DateTime FechaAusencia { get; set; }
        public int DiasAusencia { get; set; }
        public string? MotivoAusencia { get; set; }
    }
}
