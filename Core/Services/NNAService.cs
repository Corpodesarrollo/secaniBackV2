using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;
using Mapster;
using System.Reflection;

namespace Core.Services
{
    public class NNAService : INNAService
    {
        private readonly INNARepo _repository;

        public NNAService(INNARepo repository)
        {
            _repository = repository;
        }

        public async Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto)
        {
            try
            {
                var entity = GenericMapper.Map<NNADto, NNAs>(dto);
                var (success, entitys) = await _repository.AddAsync(entity);
                var dto1 = GenericMapper.Map<NNAs, NNADto>(entitys);
                var response = GenericRespuestaResponse.Response<NNADto>(success, success ? "Datos generados" : "Error al generar datos", dto1);
                return response;
            }
            catch (Exception ex)
            {
                var message = $"Error al crear contacto NNA: {ex.Message}";
                Console.WriteLine(message);
                var response = GenericRespuestaResponse.Response<NNADto>(false, message, null);
                return response;
            }

        }
    }
}
