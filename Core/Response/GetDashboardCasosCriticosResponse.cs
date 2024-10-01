namespace Core.response
{
    public class GetDashboardCasosCriticosResponse
    {

        public long AlertaId { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Diagnostico { get; set; }
        public string? DepartamentoActual { get; set; }
        public string? MunicipioActual { get; set; }
        public string? DepartamentoOrigen { get; set; }
        public string? MunicipioOrigen { get; set; }
        public DateTime? FechaNotificacionSIVIGILA { get; set; }
        public string? Entidad { get; set; }
        public DateTime? FechaSeguimiento { get; set; }

        public string? AgenteSeguimiento { get; set; }

    }
}
