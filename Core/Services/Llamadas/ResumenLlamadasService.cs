using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Llamadas;
using Core.Interfaces.Services.Llamadas;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Core.Services.Llamadas
{
    public class ResumenLlamadasService(IResumenLlamadasRepository repository) : IResumenLlamadasService
    {
        private readonly IResumenLlamadasRepository _repository = repository;

        public async Task<bool> ActualizarObservaciones(long id, [FromBody] string observaciones)
        {
            return await _repository.ActualizarObservaciones(id, observaciones);
        }

        public async Task<bool> ActualizarResumenYDetalleLlamadasAsync(Intentos intento)
        {
            return await _repository.ActualizarResumenYDetalleLlamadasAsync(intento);
        }

        public async Task<bool> EjecutarActualizacionDesdeIntentos()
        {
            return await _repository.EjecutarActualizacionDesdeIntentos();
        }

        public async Task<List<ResumenLlamadasDTO>> GetResumenLlamadas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _repository.GetResumenLlamadas(fechaInicio, fechaFin);
        }
    }
}
