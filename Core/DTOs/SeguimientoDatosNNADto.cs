namespace Core.DTOs
{
    public class SeguimientoDatosNNADto
    {
        public long IdNNA { get; set; }
        public string? NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Edad { get; set; }
        public string? Diagnostico { get; set; }
        public DateTime? FechaIngresoEstrategia { get; set; }
        public DateTime? FechaInicioSeguimiento { get; set; }
        // Bug 2026-06-17: card pintaba Fecha Inicio = MAX(Seguimiento.FechaSeguimiento) y
        // calculaba tiempo transcurrido contra esa misma fecha. PO ahora quiere medir el
        // tiempo desde la ultima actuacion (UltimaActuacionFecha), que coincide con la fecha
        // que la grilla muestra en columna "Fecha Seguimiento".
        public DateTime? UltimaActuacionFecha { get; set; }
        public string? TiempoTranscurrido { get; set; }
        public int? SeguimientosRealizados { get; set; }
        public string? Estado { get; set; }
    }
}
