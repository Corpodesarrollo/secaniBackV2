namespace Core.response
{
    public class GetContactoNNAIntentoResponse
    {
        public long Id { get; set; }
        public long NNAId { get; set; }
        public string? Nombres { get; set; }
        public int ParentescoId { get; set; }
        public string? Email { get; set; }
        public string? Telefonos { get; set; }
        public string? TelefnosInactivos { get; set; }
        public bool Cuidador { get; set; }
        public DateTime FechaIntento { get; set; }
        public int TipoResultadoIntentoId { get; set; }
        public int TipoFallaIntentoId { get; set; }
        public string? TipoFallaIntentoNombre { get; set; }
    }
}
