using Core.Modelos.Common;

namespace MSSeguimiento.Core.Modelos
{
    public class HorarioLaboralAgente : BaseEntity
    {
        public DateTime Fecha { get; set; }
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSalida { get; set; }
        public string? UserId { get; set; }
        public int Dia { get; set; }
    }
}
