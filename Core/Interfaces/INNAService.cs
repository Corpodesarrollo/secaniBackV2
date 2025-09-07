using Core.DTOs;
using Core.Modelos;
using Core.Response;

namespace Core.Interfaces
{
    public interface INNAService
    {
        //Consultas


        //Operaciones     
        Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto);
        Task<(bool, NNAs?)> UpdateAsync(NNADto dto);
    }
}
