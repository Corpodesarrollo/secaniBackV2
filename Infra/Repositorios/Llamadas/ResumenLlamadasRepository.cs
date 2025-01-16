using Core.DTOs.Reportes;
using Core.Interfaces.Repositorios.Llamadas;
using Core.Modelos;
using Core.Modelos.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.Llamadas
{
    public class ResumenLlamadasRepository(ApplicationDbContext context) : IResumenLlamadasRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> ActualizarObservaciones(long id, [FromBody] string observaciones)
        {
            var resumen = await _context.ResumenLlamadas.FindAsync(id);
            if (resumen == null)
            {
                return false;
            }

            resumen.Observaciones = observaciones;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActualizarResumenYDetalleLlamadasAsync(Intentos intento)
        {
            // Buscar el resumen de llamadas para el usuario y fecha
            var fechaInicio = intento.FechaIntento.Date;
            var fechaFin = fechaInicio.AddDays(1);

            var resumen = await _context.ResumenLlamadas
                .Include(r => r.DetallesFallas)
                .FirstOrDefaultAsync(r => r.AgenteId == intento.CreatedByUserId &&
                              r.FechaIntento >= fechaInicio &&
                              r.FechaIntento < fechaFin);

            if (resumen == null)
            {
                // Crear un nuevo resumen si no existe
                resumen = new ResumenLlamadas
                {
                    AgenteId = intento.CreatedByUserId,
                    FechaIntento = intento.FechaIntento.Date,
                    LlamadasExitosas = intento.TipoFallaIntentoId == 0 ? 1 : 0,
                    LlamadasFallidas = intento.TipoFallaIntentoId == 0 ? 0 : 1
                };

                _context.ResumenLlamadas.Add(resumen);

                if (intento.TipoFallaIntentoId != 0)
                {
                    // Agregar un nuevo detalle de falla si es una llamada fallida
                    resumen.DetallesFallas.Add(new DetalleFallasLlamadas
                    {
                        TipoFallaIntentoId = intento.TipoFallaIntentoId,
                        Cantidad = 1
                    });
                }
            }
            else
            {
                // Actualizar el resumen existente
                if (intento.TipoFallaIntentoId == 0)
                {
                    resumen.LlamadasExitosas += 1;
                }
                else
                {
                    resumen.LlamadasFallidas += 1;

                    // Actualizar o agregar el detalle de la falla
                    var detalle = resumen.DetallesFallas
                        .FirstOrDefault(d => d.TipoFallaIntentoId == intento.TipoFallaIntentoId);

                    if (detalle == null)
                    {
                        resumen.DetallesFallas.Add(new DetalleFallasLlamadas
                        {
                            TipoFallaIntentoId = intento.TipoFallaIntentoId,
                            Cantidad = 1
                        });
                    }
                    else
                    {
                        detalle.Cantidad += 1;
                    }
                }
            }

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EjecutarActualizacionDesdeIntentos()
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC ActualizarResumenDesdeIntentos");
            return true;
        }

        public async Task<List<ResumenLlamadasDTO>> GetResumenLlamadas(DateTime fechaInicio, DateTime fechaFin)
        {
            var tiposFalla = await _context.TPTipoFallaLLamada
                .Select(t => new { t.Id, t.Nombre })
                .ToListAsync();

            // Obtener los intentos agrupados por Agente y Fecha
            var resumenLlamadas = await _context.ResumenLlamadas
                .Where(r => r.FechaIntento >= fechaInicio && r.FechaIntento <= fechaFin)
                .Include(r => r.DetallesFallas)
                .ThenInclude(df => df.TipoFalla)
                .OrderBy(r => r.AgenteId)
                .ThenBy(r => r.FechaIntento)
                .ToListAsync();

            // Construir la lista de reportes
            var reporte = resumenLlamadas.Select(r => new ResumenLlamadasDTO
            {
                AgenteId = r.AgenteId,
                Agente = _context.ApplicationUser.FirstOrDefault(x => x.Id == r.AgenteId)?.UserName ?? string.Empty,
                FechaIntento = r.FechaIntento,
                LlamadasExitosas = r.LlamadasExitosas,
                LlamadasFallidas = r.LlamadasFallidas,
                DetallesFallas = tiposFalla.ToDictionary(
                    t => t.Nombre,
                    t => r.DetallesFallas.FirstOrDefault(df => df.TipoFallaIntentoId == t.Id)?.Cantidad ?? 0
                )
            }).ToList();

            return reporte;
        }
    }
}
