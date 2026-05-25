using Core.DTOs;
using Core.Modelos;
using Core.Response;
using SISPRO.TRV.Entity;

namespace Core.Interfaces
{
    public interface INNAService
    {
        //Consultas


        //Operaciones     
        Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto, User user);
        Task<(bool, NNAs?)> UpdateAsync(NNADto dto, User user);
    }
}
