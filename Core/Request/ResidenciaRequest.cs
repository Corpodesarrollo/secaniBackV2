namespace Core.Request
{
    public class ResidenciaRequest
    {
        public string? IdMunicipio { get; set; }
        public string Barrio { get; set; }
        public string? IdArea { get; set; }
        public string Direccion { get; set; }
        public string? IdEstrato { get; set; }
        public string TelefonoFijo { get; set; }
    }
}
