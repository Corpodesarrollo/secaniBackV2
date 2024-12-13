using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;

namespace Core.Services
{
    public class NNAService : INNAService
    {
        private readonly INNARepo _repository;
        private readonly IContactoNNARepo ContactoNNA;

        public NNAService(INNARepo repository, IContactoNNARepo contactoNNA)
        {
            _repository = repository;
            ContactoNNA = contactoNNA;
        }

        public async Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto)
        {
            try
            {
                dto.CreatedByUserId = "1";
                var entity = GenericMapper.Map<NNADto, NNAs>(dto);
                var (success, entitys) = await _repository.AddAsync(entity);
                if (success && dto.Contactos != null && dto.Contactos.Length > 0)
                {
                    foreach (var item in dto.Contactos)
                    {
                        item.NNAId = entitys.Id;
                        _ = await ContactoNNA.CrearContactoNNA(item);
                    }
                }

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
