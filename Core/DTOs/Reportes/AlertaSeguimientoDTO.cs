namespace Core.DTOs.Reportes
{
    public class AlertaSeguimientoDTO
    {
        public long AlertaSeguimientoId { get; set; }
        public long AlertaId { get; set; }
        public long SeguimientoId { get; set; }
        public string? Observaciones { get; set; }
        public int EstadoId { get; set; }
        public DateTime UltimaFechaSeguimiento { get; set; }
        public string NombreAlerta { get; set; }
        public string CategoriaAlerta { get; set; }
        public string SubcategoriaAlerta { get; set; }
    }
}
