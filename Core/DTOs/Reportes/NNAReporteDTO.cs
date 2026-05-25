namespace Core.DTOs.Reportes
{
    public class NNAReporteDTO
    {
        public long Id { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? SexoId { get; set; }
        public DateTime? FechaDefuncion { get; set; }
    }
}
