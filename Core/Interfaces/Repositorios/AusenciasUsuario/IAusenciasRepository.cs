using Core.DTOs.AusenciasUsuario;
using Core.Modelos;

namespace Core.Interfaces.Repositorios.AusenciasUsuario
{
    public interface IAusenciasRepository
    {
        Task<OperationResult<Ausencias>> CreateAsync(Ausencias entity, CancellationToken ct = default);
        Task<OperationResult<Ausencias>> AddAsync(Ausencias entity, CancellationToken ct = default);

        Task<OperationResult<Ausencias>> UpdateAsync(Ausencias entity, CancellationToken ct = default);

        Task<OperationResult<IReadOnlyList<Ausencias>>> GetAllAsync(CancellationToken ct = default);
        Task<OperationResult<IReadOnlyList<Ausencias>>> GetAllByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);

        Task<OperationResult<bool>> ExistsByFechaAndUsuarioIdAsync(string usuarioId, DateTime fecha, CancellationToken ct = default);

        /// <summary>Elimina definitivamente (hard delete) en BD.</summary>
        Task<OperationResult<bool>> DeleteByIdAsync(long id, CancellationToken ct = default);

    }
}