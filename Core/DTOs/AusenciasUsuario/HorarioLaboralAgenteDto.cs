namespace Core.DTOs.AusenciasUsuario
{
    public class HorarioLaboralAgenteDto
    {
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSalida { get; set; }
        public string? UserId { get; set; }
        public int Dia { get; set; }
    }

    public sealed class UpsertDiasRequestDto
    {
        public string? UserId { get; set; }
        public TimeSpan HoraEntrada { get; set; }
        public TimeSpan HoraSalida { get; set; }
        public List<int> Dias { get; set; } = new();
    }
}
