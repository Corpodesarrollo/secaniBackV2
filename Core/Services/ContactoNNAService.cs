using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;
using Mapster;

namespace Core.Services
{
    public class ContactoNNAService : IContactoNNAService
    {
        private readonly IContactoNNARepo _repository;

        public ContactoNNAService(IContactoNNARepo repository)
        {
            _repository = repository;
        }

        public async Task<RespuestaResponse<ContactoNNADto>> Obtener(long id)
        {
            var contactoNNA = await _repository.GetByIdAsync(id);
            var dto1 = GenericMapper.Map<ContactoNNA, ContactoNNADto>(contactoNNA);
            var response = GenericRespuestaResponse.Response<ContactoNNADto>(dto1.Id > 0, dto1.Id > 0 ? "Datos generados" : "Error al generar datos", dto1);
            return response;
        }

        public async Task<RespuestaResponse<ContactoNNADto>> ObtenerByNNAId(long NNAId)
        {
            var contactoNNA = await _repository.FindAllAsync(NNAId);
            var dto1 = GenericMapper.MapList<ContactoNNA, ContactoNNADto>(contactoNNA);
            var response = GenericRespuestaResponse.ResponseAll<ContactoNNADto>(dto1.Count() > 0, dto1.Count() > 0 ? "Datos generados" : "Error al generar datos", dto1);
            return response;
        }


        public async Task<RespuestaResponse<ContactoNNADto>> CrearContactoNNA(ContactoNNADto dto)
        {
            try
            {
                var entity = GenericMapper.Map<ContactoNNADto, ContactoNNA>(dto);
                var (success, contactoNNA) = await _repository.AddAsync(entity);
                var dto1 = GenericMapper.Map<ContactoNNA, ContactoNNADto>(contactoNNA);
                var response = GenericRespuestaResponse.Response<ContactoNNADto>(success, success ? "Datos generados" : "Error al generar datos", dto1);
                return response;
            }
            catch (Exception ex)
            {
                var message = $"Error al crear contacto NNA: {ex.Message}";
                Console.WriteLine(message);
                var response = GenericRespuestaResponse.Response<ContactoNNADto>(false, message, null);
                return response;
            }

        }

        public async Task<RespuestaResponse<ContactoNNADto>> ContactoNNAActualizar(ContactoNNADto dto)
        {
            try
            {
                var entity = GenericMapper.Map<ContactoNNADto, ContactoNNA>(dto);
                var (success, contactoNNA) = await _repository.UpdateAsync(entity);
                var dto1 = GenericMapper.Map<ContactoNNA, ContactoNNADto>(contactoNNA);
                var response = GenericRespuestaResponse.Response<ContactoNNADto>(success, success ? "Datos actualizados" : "Error al generar datos", dto1);
                return response;
            }
            catch (Exception ex)
            {
                var message = $"Error al actualizar contacto NNA: {ex.Message}";
                Console.WriteLine(message);
                var response = GenericRespuestaResponse.Response<ContactoNNADto>(false, message, null);
                return response;
            }

        }
    }

}
