namespace Core.DTOs
{
    // HU SECANI-RQ06-HU01: registro historico de asignacion/reasignacion en pestaña
    // Trazabilidad/Seguimiento del detalle NNA.
    public class HistorialAsignacionDto
    {
        public long Id { get; set; }
        public long SeguimientoId { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public string? AgenteAsignadoId { get; set; }
        public string? AgenteAsignadoNombre { get; set; }
        public string? CreadoPorId { get; set; }
        public string? CreadoPorNombre { get; set; }
        public string? Motivo { get; set; }
        public bool Activo { get; set; }
    }
}
