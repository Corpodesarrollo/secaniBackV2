using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;


namespace Infra.Repositorios
{
    public class ContactoNNARepo : IContactoNNARepo
    {
        private readonly ApplicationDbContext _context;
        private readonly GenericRepository<ContactoNNA> _repository;

        public ContactoNNARepo(ApplicationDbContext context)
        {
            _context = context;
            GenericRepository<ContactoNNA> repository = new(_context);
            _repository = repository;
        }

        public async Task<RespuestaResponse<ContactoNNADto>> Obtener(long id)
        {
            var contactoNNA = await _repository.GetByIdAsync(id);
            if (contactoNNA == null)
                return GenericRespuestaResponse.Response<ContactoNNADto>(false, "Contacto no encontrado", null);

            var dto1 = GenericMapper.Map<ContactoNNA, ContactoNNADto>(contactoNNA);
            var response = GenericRespuestaResponse.Response(dto1.Id > 0, dto1.Id > 0 ? "Datos generados" : "Error al generar datos", dto1);
            return response;
        }

        public async Task<RespuestaResponse<List<ContactoNNADto>>> ObtenerByNNAId(long NNAId)
        {
            var contactoNNA = await (from c in _context.ContactoNNAs
                                     join p in _context.TPParentescos on c.ParentescoId equals p.Id
                                     where c.NNAId == NNAId
                                     select new ContactoNNADto
                                     {
                                         Id = c.Id,
                                         NNAId = c.NNAId,
                                         Nombres = c.Nombres,
                                         ParentescoId = c.ParentescoId,
                                         Parentesco = p.Nombre,
                                         Email = c.Email,
                                         Telefonos = c.Telefonos,
                                         TelefnosInactivos = c.TelefnosInactivos,
                                         Cuidador = c.Cuidador
                                     }).ToListAsync();


            var response = GenericRespuestaResponse.ResponseAll(contactoNNA.Count() > 0, contactoNNA.Count() > 0 ? "Datos generados" : "Error al generar datos", contactoNNA);
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
                var result = await _context.ContactoNNAs.FirstOrDefaultAsync(x => x.Id == dto.Id);
                if (result == null)
                    return GenericRespuestaResponse.Response<ContactoNNADto>(false, " Contacto no encontrado.", null);

                result.DateUpdated = DateTime.Now;
                result.Nombres = dto.Nombres;
                result.ParentescoId = dto.ParentescoId;
                result.Email = dto.Email;
                result.Telefonos = dto.Telefonos;
                result.TelefnosInactivos = dto.TelefnosInactivos;
                result.Cuidador = dto.Cuidador;
                result.UpdatedByUserId = dto.UpdatedByUserId;
                result.Estado = dto.Estado;

                _context.ContactoNNAs.Update(result);
                var success = await _context.SaveChangesAsync() > 0;
                var response = GenericRespuestaResponse.Response<ContactoNNADto>(success, success ? "Datos actualizados" : "Error al generar datos", dto);
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