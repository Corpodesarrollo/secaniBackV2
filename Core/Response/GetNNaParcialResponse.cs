namespace Core.response
{
    public class GetNNaParcialResponse
    {
        public long Id { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime? FechaNotificacionSIVIGILA { get; set; }
    }
}