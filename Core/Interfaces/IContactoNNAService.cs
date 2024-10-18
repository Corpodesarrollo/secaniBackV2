using Core.DTOs;
using Core.Interfaces.MSTablasParametricas;
using Core.Modelos;
using Core.Response;
using System.Reflection;

namespace Core.Interfaces
{
    public interface IContactoNNAService 
    {
        //Consultas
        Task<RespuestaResponse<ContactoNNADto>> Obtener(long id);
        Task<RespuestaResponse<ContactoNNADto>> ObtenerByNNAId(long NNAId);

        //Operaciones   
        Task<RespuestaResponse<ContactoNNADto>> CrearContactoNNA(ContactoNNADto dto);
        Task<RespuestaResponse<ContactoNNADto>> ContactoNNAActualizar(ContactoNNADto dto);

        
    }
}
