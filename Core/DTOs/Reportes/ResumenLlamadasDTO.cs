using Core.Modelos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.DTOs.Reportes
{
    public class ResumenLlamadasDTO
    {
        [Key]
        public long Id { get; set; }

        public string AgenteId { get; set; } //CreatedByUserId

        public string Agente { get; set; }

        public DateTime FechaIntento { get; set; }

        public int LlamadasExitosas { get; set; }

        public int LlamadasFallidas { get; set; }

        public string? Observaciones { get; set; }

        // Relación con los detalles de fallas
        public IDictionary<string, int> DetallesFallas { get; set; }
    }

    public class DetalleFallasLlamadasDTO
    {
        [Key]
        public long Id { get; set; }

        public int TipoFallaIntentoId { get; set; }

        public string TipoFallaIntento { get; set; }

        public int Cantidad { get; set; }
    }
}
