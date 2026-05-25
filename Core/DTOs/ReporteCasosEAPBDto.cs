namespace Core.DTOs
{
    public class ReporteCasosEAPBDto
    {
        public DateTime? FechaNotificacion { get; set; }
        public string? NombreNNA { get; set; }
        public string? Edad { get; set; }
        public string? Sexo { get; set; }
        public int TiempoTranscurrido { get; set; }
        public string? Estado { get; set; }
    }
}
