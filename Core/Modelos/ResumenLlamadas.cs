using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class ResumenLlamadas
    {
        [Key]
        public long Id { get; set; }

        public string AgenteId { get; set; } //CreatedByUserId

        public DateTime FechaIntento { get; set; }

        public int LlamadasExitosas { get; set; }

        public int LlamadasFallidas { get; set; }

        public string? Observaciones { get; set; }

        // Relación con los detalles de fallas
        public ICollection<DetalleFallasLlamadas> DetallesFallas { get; set; } = new List<DetalleFallasLlamadas>();
    }

    public class DetalleFallasLlamadas
    {
        [Key]
        public long Id { get; set; }

        public int TipoFallaIntentoId { get; set; }

        [ForeignKey(nameof(TipoFallaIntentoId))]
        public TPTipoFallaLLamada TipoFalla { get; set; }

        public int Cantidad { get; set; }

        // Relación con ResumenLlamadas
        public long ResumenLlamadasId { get; set; }
        [ForeignKey(nameof(ResumenLlamadasId))]
        public ResumenLlamadas Resumen { get; set; }
    }

}
