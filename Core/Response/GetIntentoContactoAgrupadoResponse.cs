namespace Core.response
{
    public class GetIntentoContactoAgrupadoResponse
    {
        public long Id { get; set; }
        public long NNAId { get; set; }
        public string? Nombres { get; set; }
        public int? ParentescoId { get; set; }
        public string? Email { get; set; }
        public string? Telefonos { get; set; }
        public string? TelefnosInactivos { get; set; }
        public bool Cuidador { get; set; }
        public string? TipoResultadoIntento1 { get; set; }
        public string? TipoResultadoIntento2 { get; set; }
    }
}
