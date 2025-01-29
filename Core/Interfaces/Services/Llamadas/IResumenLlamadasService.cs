using Core.DTOs.Reportes;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace Core.Interfaces.Services.Llamadas
{
    public interface IResumenLlamadasService
    {
        Task<List<ResumenLlamadasDTO>> GetResumenLlamadas(DateTime fechaInicio, DateTime fechaFin);
        Task<bool> ActualizarResumenYDetalleLlamadasAsync(Intentos intento);
        Task<bool> ActualizarObservaciones(long id, [FromBody] string observaciones);
        Task<bool> EjecutarActualizacionDesdeIntentos();
    }
}
