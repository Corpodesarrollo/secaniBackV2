namespace Core.DTOs.MSTablasParametricas
{
    public class FestivoDTO
    {
        public long Id { get; set; }
        public DateOnly Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
    }
}
