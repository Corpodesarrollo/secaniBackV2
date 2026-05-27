using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Repositorios.AusenciasUsuario;
using Core.Interfaces.Services.AusenciasUsuario;
using MSSeguimiento.Core.Modelos;
using static Core.Common.Estructuras;

namespace Core.Services.AusenciasService
{
    public class HorarioLaboralAgenteService : IHorarioLaboralAgenteService
    {
        private readonly IHorarioLaboralAgenteRepository _repo;
        private readonly INotificacionRepo _notificacionRepo;

        public HorarioLaboralAgenteService(IHorarioLaboralAgenteRepository repo, INotificacionRepo notificacionRepo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _notificacionRepo = notificacionRepo;
        }

        // BUG-LZ-079: notificar al coordinador cuando el agente programa/actualiza su horario.
        private async Task NotificarProgramacionHorarioAsync(string usuarioId)
        {
            try
            {
                await _notificacionRepo.SetNotificacion(new()
                {
                    IdAgenteOrigen = usuarioId,
                    TipoNotificacion = TipoNotificacion.ProgramacionHorario,
                    TextoNotificacion = "ha programado/actualizado su horario laboral de la semana",
                    FechaNotificacion = DateTime.UtcNow
                });
            }
            catch
            {
                // la notificacion no debe tumbar el guardado del horario
            }
        }

        // ---------------- Mapeos ----------------
        private static HorarioLaboralAgente ToEntity(HorarioLaboralAgenteDto dto) => new()
        {
            UserId = dto.UserId,
            Dia = dto.Dia,
            HoraEntrada = dto.HoraEntrada,
            HoraSalida = dto.HoraSalida
        };

        private static HorarioLaboralAgenteDto ToDto(HorarioLaboralAgente e) => new()
        {
            UserId = e.UserId,
            Dia = e.Dia,
            HoraEntrada = e.HoraEntrada,
            HoraSalida = e.HoraSalida
        };

        private static IReadOnlyList<HorarioLaboralAgenteDto> ToDtoList(IReadOnlyList<HorarioLaboralAgente> list)
            => list.Select(ToDto).ToList();

        // ---------------- Operaciones ----------------

        public async Task<OperationResult<HorarioLaboralAgenteDto>> UpsertDayAsync(HorarioLaboralAgenteDto dto, CancellationToken ct = default)
        {
            if (dto is null)
                return OperationResult<HorarioLaboralAgenteDto>.Fail("ARG_NULL", "El DTO no puede ser nulo.");

            var repoResult = await _repo.UpsertDayAsync(ToEntity(dto), ct);
            if (repoResult.Success)
                await NotificarProgramacionHorarioAsync(dto.UserId);
            return repoResult.Map(ToDto);
        }

        public Task<OperationResult<bool>> DeleteDayAsync(string userId, int dia, CancellationToken ct = default)
            => _repo.DeleteDayAsync(userId, dia, ct);

        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> GetScheduleAsync(string userId, CancellationToken ct = default)
        {
            var repoResult = await _repo.GetScheduleAsync(userId, ct);
            return repoResult.Map(ToDtoList);
        }

        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> UpsertDaysAsync(
            string userId, int diaInicial, int diaFinal, TimeSpan horaEntrada, TimeSpan horaSalida, CancellationToken ct = default)
        {
            var repoResult = await _repo.UpsertDaysAsync(userId, diaInicial, diaFinal, horaEntrada, horaSalida, ct);
            if (repoResult.Success)
                await NotificarProgramacionHorarioAsync(userId);
            return repoResult.Map(ToDtoList);
        }

        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgenteDto>>> DeleteDaysAsync(
            string userId, int diaInicial, int diaFinal, CancellationToken ct = default)
        {
            var repoResult = await _repo.DeleteDaysAsync(userId, diaInicial, diaFinal, ct);
            return repoResult.Map(ToDtoList);
        }
    }
}
