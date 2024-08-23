namespace Core.DTOs
{
    public class SeguimientoDto
    {
        public long Id { get; set; }
        public long? NoCaso { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? NombreCompleto
        {
            get
            {
                return $"{PrimerNombre} {SegundoNombre} {PrimerApellido} {SegundoApellido}";
            }
        }
        public TPEstadoNNADto? Estado { get; set; }
        public string? AsuntoUltimaActuacion { get; set; }
        public DateTime? FechaUltimaActuacion { get; set; }
        public List<AlertaSeguimientoDto>? Alertas { get; set; }
    }
}
