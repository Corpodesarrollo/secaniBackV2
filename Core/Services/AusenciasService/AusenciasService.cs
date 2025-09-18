using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Repositorios.AusenciasUsuario;
using Core.Interfaces.Services.AusenciasUsuario;
using Core.Modelos;

namespace Core.Services.AusenciasService
{
    public class AusenciasService : IAusenciasService
    {
        private readonly IAusenciasRepository _repo;

        public AusenciasService(IAusenciasRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        // ---------------- Mapeos ----------------
        private static Ausencias ToEntity(AusenciaDto dto) => new()
        {
            Id = dto.Id,
            UsuarioId = dto.UsuarioId,
            FechaAusencia = dto.FechaAusencia, // el repo normaliza .Date
            DiasAusencia = 1,                 // regla de negocio
            MotivoAusencia = dto.MotivoAusencia
        };

        private static AusenciaDto ToDto(Ausencias e) => new()
        {
            Id = e.Id,
            UsuarioId = e.UsuarioId,
            FechaAusencia = e.FechaAusencia,
            DiasAusencia = 1, // regla de negocio
            MotivoAusencia = e.MotivoAusencia
        };

        private static IReadOnlyList<AusenciaDto> ToDtoList(IReadOnlyList<Ausencias> list)
            => list.Select(ToDto).ToList();

        // ---------------- Operaciones ----------------

        public async Task<OperationResult<AusenciaDto>> CreateAsync(AusenciaDto dto, CancellationToken ct = default)
        {
            if (dto is null)
                return OperationResult<AusenciaDto>.Fail("ARG_NULL", "El DTO no puede ser nulo.");

            var entity = ToEntity(dto);
            var result = await _repo.CreateAsync(entity, ct);
            return result.Map(ToDto);
        }

        public Task<OperationResult<AusenciaDto>> AddAsync(AusenciaDto dto, CancellationToken ct = default)
            => CreateAsync(dto, ct); // alias

        public async Task<OperationResult<AusenciaDto>> UpdateAsync(AusenciaDto dto, CancellationToken ct = default)
        {
            if (dto is null)
                return OperationResult<AusenciaDto>.Fail("ARG_NULL", "El DTO no puede ser nulo.");

            var entity = ToEntity(dto);
            var result = await _repo.UpdateAsync(entity, ct);
            return result.Map(ToDto);
        }

        public async Task<OperationResult<IReadOnlyList<AusenciaDto>>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _repo.GetAllAsync(ct);
            return result.Map(ToDtoList);
        }

        public async Task<OperationResult<IReadOnlyList<AusenciaDto>>> GetAllByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        {
            var result = await _repo.GetAllByUsuarioIdAsync(usuarioId, ct);
            return result.Map(ToDtoList);
        }

        public Task<OperationResult<bool>> ExistsByFechaAndUsuarioIdAsync(string usuarioId, DateTime fecha, CancellationToken ct = default)
            => _repo.ExistsByFechaAndUsuarioIdAsync(usuarioId, fecha, ct);

        public Task<OperationResult<bool>> DeleteByIdAsync(long id, CancellationToken ct = default)
            => _repo.DeleteByIdAsync(id, ct);
    }
}