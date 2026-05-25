using Core.DTOs.AusenciasUsuario;

namespace Core.Interfaces.Services.AusenciasUsuario
{
    public interface IAusenciasService
    {
        Task<OperationResult<AusenciaDto>> CreateAsync(AusenciaDto dto, CancellationToken ct = default);
        Task<OperationResult<AusenciaDto>> AddAsync(AusenciaDto dto, CancellationToken ct = default); // alias

        Task<OperationResult<AusenciaDto>> UpdateAsync(AusenciaDto dto, CancellationToken ct = default);

        Task<OperationResult<IReadOnlyList<AusenciaDto>>> GetAllAsync(CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<AusenciaDto>>> GetAllByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);

        Task<OperationResult<bool>> ExistsByFechaAndUsuarioIdAsync(string usuarioId, DateTime fecha, CancellationToken ct = default);

        Task<OperationResult<bool>> DeleteByIdAsync(long id, CancellationToken ct = default);
    }
}
