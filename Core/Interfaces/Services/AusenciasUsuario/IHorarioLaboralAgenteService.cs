using Core.DTOs.AusenciasUsuario;

namespace Core.Interfaces.Services.AusenciasUsuario
{
    public interface IHorarioLaboralAgenteService
    {
        Task<OperationResult<HorarioLaboralAgenteDto>> UpsertDayAsync(HorarioLaboralAgenteDto dto, CancellationToken ct = default);
        Task<OperationResult<bool>> DeleteDayAsync(string userId, int dia, CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> GetScheduleAsync(string userId, CancellationToken ct = default);

        Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> UpsertDaysAsync(
            string userId, int diaInicial, int diaFinal, TimeSpan horaEntrada, TimeSpan horaSalida, CancellationToken ct = default);

        Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> DeleteDaysAsync(
            string userId, int diaInicial, int diaFinal, CancellationToken ct = default);
    }
}
