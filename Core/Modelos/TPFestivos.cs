namespace Core.Modelos
{
    public class TPFestivos
    {
        public long Id { get; set; }
        public DateTime Festivo { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int Anio()
        {
            return Festivo.Year;
        }
    }
}
