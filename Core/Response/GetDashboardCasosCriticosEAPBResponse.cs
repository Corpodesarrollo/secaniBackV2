namespace Core.response
{
    public class GetDashboardCasosCriticosEapbResponse
    {

        public long AlertaId { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Diagnostico { get; set; }
   
        public DateTime? FechaSeguimiento { get; set; }

    }
}
