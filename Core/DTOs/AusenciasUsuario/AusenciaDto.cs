namespace Core.DTOs.AusenciasUsuario
{
    public sealed class AusenciaDto
    {
        public long Id { get; set; }
        public string? UsuarioId { get; set; }
        public DateTime FechaAusencia { get; set; }
        public int DiasAusencia { get; set; } = 1; 
        public string? MotivoAusencia { get; set; }
    }
}
