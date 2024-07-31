namespace Core.response
{
    public class GetSeguimientoHorarioAgenteResponse
    {
        public DateTime Fecha { get; set; }
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSalida { get; set; }

    }
}
