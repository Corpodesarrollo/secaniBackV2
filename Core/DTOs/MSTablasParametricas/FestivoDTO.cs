namespace Core.DTOs.MSTablasParametricas
{
    public class FestivoDTO
    {
        public long Id { get; set; }
        public DateTime Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }

    public class UpdateFestivoRequest
    {
        public long Id { get; set; }
        public DateOnly Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int Orden { get; set; } = 0;
    }

    public class CreateFestivoRequest
    {
        public long Id { get; set; }
        public DateOnly Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int Orden { get; set; } = 0;
    }
}
