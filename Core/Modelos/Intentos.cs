using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Intentos : BaseEntity
    {
        public long ContactoNNAId { get; set; }
        public string Email { get; set; }
        public DateTime FechaIntento { get; set; }
        public string Telefono { get; set; }
        public int TipoResultadoIntentoId { get; set; }
        public int TipoFallaIntentoId { get; set; }
    }
}
