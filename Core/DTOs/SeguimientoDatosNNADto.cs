namespace Core.DTOs
{
    public class SeguimientoDatosNNADto
    {
        public int IdNNA { get; set; }
        public string? NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Edad { get; set; }
        public string? Diagnostico { get; set; }
        public DateTime? FechaIngresoEstrategia { get; set; }
        public DateTime? FechaInicioSeguimiento { get; set; }
        public string? TiempoTranscurrido { get; set; }
        public int? SeguimientosRealizados { get; set; }
    }
}
