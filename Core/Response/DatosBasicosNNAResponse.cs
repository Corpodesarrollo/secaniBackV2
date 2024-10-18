namespace Core.Response
{
    public class DatosBasicosNNAResponse
    {
        public string NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Diagnostico { get; set; }
        public DateTime? FechaInicioSegumiento { get; set; }
        public int? DiagnosticoId { get; set; }
    }
}
