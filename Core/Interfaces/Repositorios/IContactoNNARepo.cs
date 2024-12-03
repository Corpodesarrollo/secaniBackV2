using Core.DTOs;
using Core.Response;

namespace Core.Interfaces.Repositorios
{
    public interface IContactoNNARepo
    {
        Task<RespuestaResponse<ContactoNNADto>> ContactoNNAActualizar(ContactoNNADto dto);
        Task<RespuestaResponse<ContactoNNADto>> CrearContactoNNA(ContactoNNADto dto);
        Task<RespuestaResponse<ContactoNNADto>> Obtener(long id);
        Task<RespuestaResponse<List<ContactoNNADto>>> ObtenerByNNAId(long NNAId);
    }
}
