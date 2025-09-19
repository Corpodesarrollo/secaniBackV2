using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Repositorios.AusenciasUsuario;
using Microsoft.EntityFrameworkCore;
using MSSeguimiento.Core.Modelos;

namespace Infra.Repositorios.AusenciasUsuario
{
    public class HorarioLaboralAgenteRepository : IHorarioLaboralAgenteRepository
    {
        private readonly ApplicationDbContext _context;

        public HorarioLaboralAgenteRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // -------- Helpers de validación --------
        private static bool IsValidDia(int dia) => dia >= 0 && dia <= 6;
        private static bool IsZero(TimeSpan t) => t == TimeSpan.Zero;

        private static OperationError Err(string code, string message, string? field = null)
            => new(code, message, field);

        private static bool IsActiveRange(TimeSpan entrada, TimeSpan salida)
            => !IsZero(entrada) || !IsZero(salida);

        private static bool IsValidActiveRange(TimeSpan entrada, TimeSpan salida)
            => entrada < salida; // regla: si activo, debe ser rango válido

        // -------- Upsert de un día --------
        public async Task<OperationResult<HorarioLaboralAgente>> UpsertDayAsync(HorarioLaboralAgente entity, CancellationToken ct = default)
        {
            if (entity is null)
                return OperationResult<HorarioLaboralAgente>.Fail(Err("ARG_NULL", "La entidad no puede ser nula."));

            if (string.IsNullOrWhiteSpace(entity.UserId))
                return OperationResult<HorarioLaboralAgente>.Fail(Err("USER_REQUERIDO", "UserId es obligatorio.", "UserId"));

            if (!IsValidDia(entity.Dia))
                return OperationResult<HorarioLaboralAgente>.Fail(Err("DIA_INVALIDO", "Dia debe estar entre 0 y 6.", "Dia"));

            // Validación de horas: si es "activo" (no 00:00–00:00), debe cumplir entrada < salida
            if (IsActiveRange(entity.HoraEntrada, entity.HoraSalida) &&
                !IsValidActiveRange(entity.HoraEntrada, entity.HoraSalida))
            {
                return OperationResult<HorarioLaboralAgente>.Fail(Err("RANGO_INCORRECTO", "HoraEntrada debe ser menor que HoraSalida para días activos.", "HoraEntrada"));
            }

            // Upsert por (UserId, Dia)
            var current = await _context.HorarioLaboralAgente
                .FirstOrDefaultAsync(x => x.UserId == entity.UserId && x.Dia == entity.Dia, ct);

            if (current is null)
            {
                // Crear nuevo
                var nuevo = new HorarioLaboralAgente
                {
                    UserId = entity.UserId,
                    Dia = entity.Dia,
                    HoraEntrada = entity.HoraEntrada,
                    HoraSalida = entity.HoraSalida,
                    Fecha = DateTime.UtcNow.Date
                };

                await _context.HorarioLaboralAgente.AddAsync(nuevo, ct);
                await _context.SaveChangesAsync(ct);

                return OperationResult<HorarioLaboralAgente>.Ok(nuevo, "Horario creado.");
            }
            else
            {
                // Modificar existente
                current.HoraEntrada = entity.HoraEntrada;
                current.HoraSalida = entity.HoraSalida;
                // Fecha no interviene en la lógica; si quieres refrescar, puedes mantener la original
                await _context.SaveChangesAsync(ct);

                return OperationResult<HorarioLaboralAgente>.Ok(current, "Horario actualizado.");
            }
        }

        // -------- DeleteDay: pone 00:00–00:00 --------
        public async Task<OperationResult<bool>> DeleteDayAsync(string userId, int dia, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return OperationResult<bool>.Fail(Err("USER_REQUERIDO", "UserId es obligatorio.", "UserId"));

            if (!IsValidDia(dia))
                return OperationResult<bool>.Fail(Err("DIA_INVALIDO", "Dia debe estar entre 0 y 6.", "Dia"));

            var current = await _context.HorarioLaboralAgente
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Dia == dia, ct);

            if (current is null)
            {
                // Si no existe, creamos en cero para mantener consistencia de consulta
                current = new HorarioLaboralAgente
                {
                    UserId = userId,
                    Dia = dia,
                    HoraEntrada = TimeSpan.Zero,
                    HoraSalida = TimeSpan.Zero,
                    Fecha = DateTime.UtcNow.Date
                };
                await _context.HorarioLaboralAgente.AddAsync(current, ct);
            }
            else
            {
                current.HoraEntrada = TimeSpan.Zero;
                current.HoraSalida = TimeSpan.Zero;
            }

            await _context.SaveChangesAsync(ct);
            return OperationResult<bool>.Ok(true, "Día marcado como inactivo (00:00–00:00).");
        }

        // -------- GetSchedule: garantiza 7 días --------
        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> GetScheduleAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(Err("USER_REQUERIDO", "UserId es obligatorio.", "UserId"));

            var existentes = await _context.HorarioLaboralAgente
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);

            var porDia = existentes.ToDictionary(x => x.Dia, x => x);
            var lista = new List<HorarioLaboralAgente>(capacity: 7);

            for (int d = 0; d <= 6; d++)
            {
                if (porDia.TryGetValue(d, out var h))
                {
                    lista.Add(h);
                }
                else
                {
                    lista.Add(new HorarioLaboralAgente
                    {
                        UserId = userId,
                        Dia = d,
                        HoraEntrada = TimeSpan.Zero,
                        HoraSalida = TimeSpan.Zero,
                        Fecha = DateTime.UtcNow.Date
                    });
                }
            }

            return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Ok(lista);
        }

        // -------- UpsertDays (bulk) --------
        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> UpsertDaysAsync(
            string userId, int diaInicial, int diaFinal, TimeSpan horaEntrada, TimeSpan horaSalida, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("USER_REQUERIDO", "UserId es obligatorio.", "UserId"));

            if (!IsValidDia(diaInicial) || !IsValidDia(diaFinal))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("DIA_INVALIDO", "Los días deben estar entre 0 y 6.", "Dia"));

            if (diaFinal < diaInicial)
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("RANGO_INVALIDO", "DiaFinal no puede ser menor que DiaInicial.", "DiaFinal"));

            // Validación de rango activo
            if (IsActiveRange(horaEntrada, horaSalida) && !IsValidActiveRange(horaEntrada, horaSalida))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("RANGO_INCORRECTO", "HoraEntrada debe ser menor que HoraSalida para días activos.", "HoraEntrada"));

            // Construimos la lista de días desde diaInicial hasta diaFinal (inclusive)
            var diasRango = Enumerable.Range(diaInicial, diaFinal - diaInicial + 1).ToList();

            var existentes = await _context.HorarioLaboralAgente
                .Where(x => x.UserId == userId && diasRango.Contains(x.Dia))
                .ToListAsync(ct);

            var porDia = existentes.ToDictionary(x => x.Dia, x => x);
            var result = new List<HorarioLaboralAgente>(diasRango.Count);

            foreach (var dia in diasRango)
            {
                if (porDia.TryGetValue(dia, out var current))
                {
                    // Actualizar existente
                    current.HoraEntrada = horaEntrada;
                    current.HoraSalida = horaSalida;
                    result.Add(current);
                }
                else
                {
                    // Crear nuevo
                    var nuevo = new HorarioLaboralAgente
                    {
                        UserId = userId,
                        Dia = dia,
                        HoraEntrada = horaEntrada,
                        HoraSalida = horaSalida,
                        Fecha = DateTime.UtcNow.Date
                    };
                    await _context.HorarioLaboralAgente.AddAsync(nuevo, ct);
                    result.Add(nuevo);
                }
            }

            await _context.SaveChangesAsync(ct);
            return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Ok(result, "Días actualizados/creados.");
        }

        public async Task<OperationResult<IReadOnlyList<HorarioLaboralAgente>>> DeleteDaysAsync(
            string userId, int diaInicial, int diaFinal, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("USER_REQUERIDO", "UserId es obligatorio.", "UserId"));

            if (!IsValidDia(diaInicial) || !IsValidDia(diaFinal))
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("DIA_INVALIDO", "Los días deben estar entre 0 y 6.", "Dia"));

            if (diaFinal < diaInicial)
                return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Fail(
                    Err("RANGO_INVALIDO", "DiaFinal no puede ser menor que DiaInicial.", "DiaFinal"));

            var diasRango = Enumerable.Range(diaInicial, diaFinal - diaInicial + 1).ToList();

            // Traer existentes del rango
            var existentes = await _context.HorarioLaboralAgente
                .Where(x => x.UserId == userId && diasRango.Contains(x.Dia))
                .ToListAsync(ct);

            // Para mantener consistencia, si algún día no existe, lo creamos en cero
            var porDia = existentes.ToDictionary(x => x.Dia, x => x);
            var result = new List<HorarioLaboralAgente>(diasRango.Count);

            foreach (var dia in diasRango)
            {
                if (porDia.TryGetValue(dia, out var current))
                {
                    current.HoraEntrada = TimeSpan.Zero;
                    current.HoraSalida = TimeSpan.Zero;
                    result.Add(current);
                }
                else
                {
                    var nuevo = new HorarioLaboralAgente
                    {
                        UserId = userId,
                        Dia = dia,
                        HoraEntrada = TimeSpan.Zero,
                        HoraSalida = TimeSpan.Zero,
                        Fecha = DateTime.UtcNow.Date
                    };
                    await _context.HorarioLaboralAgente.AddAsync(nuevo, ct);
                    result.Add(nuevo);
                }
            }

            await _context.SaveChangesAsync(ct);
            return OperationResult<IReadOnlyList<HorarioLaboralAgente>>.Ok(result, "Días marcados como inactivos (00:00–00:00).");
        }
    }
}