namespace Core.Modelos
{
    public class TPFestivos
    {
        public long Id { get; set; }
        public DateTime Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public DateTime? FechaCreacion { get; set; } = DateTime.Now;
        public int Orden { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public int Anio()
        {
            return Festivo.Year;
        }
    }
}
