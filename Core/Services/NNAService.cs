using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositorios;
using Core.Interfaces.Services.Reportes;
using Core.Modelos;
using Core.Modelos.Common;
using Core.Response;
using SISPRO.TRV.Entity;

namespace Core.Services
{
    public class NNAService : INNAService
    {
        private readonly INNARepo _repository;
        private readonly IContactoNNARepo ContactoNNA;
        private readonly IReporteInconsistenciaPersonaService _reporteInconsistenciaPersonaService;

        public NNAService(INNARepo repository, IContactoNNARepo contactoNNA, IReporteInconsistenciaPersonaService reporteInconsistenciaPersonaService)
        {
            _repository = repository;
            ContactoNNA = contactoNNA;
            _reporteInconsistenciaPersonaService = reporteInconsistenciaPersonaService;
        }

        public async Task<RespuestaResponse<NNADto>> AddAsync(NNADto dto, User user)
        {
            var transaction = await _repository.BeginTransactionAsync();
            try
            {
                var entity = GenericMapper.Map<NNADto, NNAs>(dto);
                var (success, entitys) = await _repository.AddAsync(entity, user);

                long? primerContactoId = null;
                string? primerTelefono = null;
                if (success && dto.Contactos != null && dto.Contactos.Length > 0)
                {
                    foreach (var item in dto.Contactos)
                    {
                        item.NNAId = entitys.Id;
                        var resCont = await ContactoNNA.CrearContactoNNA(item);
                        if (primerContactoId == null && resCont?.Datos?.Id > 0)
                        {
                            primerContactoId = resCont.Datos.Id;
                            primerTelefono = resCont.Datos.Telefonos;
                        }
                    }
                }

                var dto1 = GenericMapper.Map<NNAs, NNADto>(entitys);

                // BUG-026: crear Seguimiento inicial + UsuarioAsignados (paralelo a cargue masivo)
                if (success)
                {
                    await _repository.CrearSeguimientoInicialNNA(entitys.Id, primerContactoId, primerTelefono, entity.CreatedByUserId);
                }

                // BUG-LZ-004: reporte inconsistencia es side-effect, no debe romper save NNA
                try
                {
                    await _reporteInconsistenciaPersonaService.AddReporteInconsistenciaAsync(dto1);
                }
                catch (Exception exReporte)
                {
                    Console.WriteLine($"WARN: reporte inconsistencia falló (no bloquea save NNA): {exReporte.Message}");
                }

                await _repository.CommitTransactionAsync(transaction);

                var response = GenericRespuestaResponse.Response<NNADto>(success, success ? "Datos generados" : "Error al generar datos", dto1);
                return response;
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync(transaction);
                var message = $"Error al crear contacto NNA: {ex.Message}";
                Console.WriteLine(message);
                var response = GenericRespuestaResponse.Response<NNADto>(false, message, null);
                return response;
            }

        }

        public async Task<(bool, NNAs?)> UpdateAsync(NNADto dto, User user)
        {
            var transaction = await _repository.BeginTransactionAsync();
            try
            {
                var entity = GenericMapper.Map<NNADto, NNAs>(dto);
                entity.TrasladosIPSId = dto.TrasladosIPSId != null
                    ? string.Join(",", dto.TrasladosIPSId)
                    : string.Empty;
                var (success, entitys) = await _repository.UpdateAsync(entity, user);

                //si el estado es fallecido [10], el seguimiento cambia a estado culminado [3]
                if (success && dto.estadoId == 10)
                    await _repository.ActualizarFallecido(dto);

                await _repository.CommitTransactionAsync(transaction);

                return (success, entitys);
            }
            catch (Exception ex)
            {
                await _repository.RollbackTransactionAsync(transaction);
                throw;
            }
        }
    }
}
