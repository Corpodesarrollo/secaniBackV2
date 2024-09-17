namespace Core.Modelos
{
    public class Ausencias
    {
        public long Id { get; set; }
        public string? UsuarioId { get; set; }
        public DateTime FechaAusencia { get; set; }
        public int DiasAusencia { get; set; }
        public string? MotivoAusencia { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime DateCreated { get; set; }
        public string? DeletedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public string? UpdatedByUserId { get; set; }

        
    }
}
