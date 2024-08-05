using Core.DTOs.MSPermisos;

namespace Core.Services.MSPermisos
{
    public interface IFuncionalidadService
    {
        //Queries
        Task<FuncionalidadResponseDTO> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<IEnumerable<FuncionalidadResponseDTO>> GetAllAsync(CancellationToken cancellationToken);

        //Commands
        Task<(bool, FuncionalidadResponseDTO)> AddAsync(FuncionalidadRequestDTO entity, CancellationToken cancellationToken);
        Task<(bool, FuncionalidadResponseDTO)> UpdateAsync(FuncionalidadResponseDTO entity, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(FuncionalidadResponseDTO entity, CancellationToken cancellationToken);
    }
}
