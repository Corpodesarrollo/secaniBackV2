namespace Core.DTOs
{
    public class ControlProcesosAutomaticosDto
    {
        public int Id { get; set; }
        public string? NombreProceso { get; set; }
        public DateTime? UltimaEjecucion { get; set; }
        public DateTime? ProximaEjecucion { get; set; }
        public int IntervaloSegundos { get; set; }
        public bool EstaActivo { get; set; }
        public bool EnEjecucion { get; set; }
        public string? Estado { get; set; }
        public string? MensajeError { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
