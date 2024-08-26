using Core.DTOs;
using Core.Response;
using System.Reflection;

namespace Core.Interfaces
{
    public interface INNAService
    {
        //Consultas
        

        //Operaciones     
        Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto);   
    }
}
