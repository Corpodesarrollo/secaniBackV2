namespace Core.response
{
    public class GetIntentoResponse
    {
        public long Id { get; set; }
        public long ContactoNNAId { get; set; }
        public string? Email { get; set; }
        public DateTime FechaIntento { get; set; }
        public string? Telefono { get; set; }
        public int TipoResultadoIntentoId { get; set; }
        public int TipoFallaIntentoId { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? DeletedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public string? UpdatedByUserId { get; set; }
    }
}
