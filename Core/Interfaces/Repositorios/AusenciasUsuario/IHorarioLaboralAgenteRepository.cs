using Core.DTOs.AusenciasUsuario;
using MSSeguimiento.Core.Modelos;

namespace Core.Interfaces.Repositorios.AusenciasUsuario
{
    public interface IHorarioLaboralAgenteRepository
    {
        Task<OperationResult<HorarioLaboralAgente>> UpsertDayAsync(HorarioLaboralAgente entity, CancellationToken ct = default);
        Task<OperationResult<bool>> DeleteDayAsync(string userId, int dia, CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> GetScheduleAsync(string userId, CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> UpsertDaysAsync(string userId, int diaInicial, int diaFinal, TimeSpan horaEntrada, TimeSpan horaSalida, CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> DeleteDaysAsync(string userId, int diaInicial, int diaFinal, CancellationToken ct = default);
    }
}
