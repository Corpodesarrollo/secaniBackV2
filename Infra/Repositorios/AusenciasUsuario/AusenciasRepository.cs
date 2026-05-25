using Core.DTOs.AusenciasUsuario;
using Core.Interfaces.Repositorios.AusenciasUsuario;
using Core.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios.AusenciasUsuario
{
    public sealed class AusenciasRepository : IAusenciasRepository
    {
        private readonly ApplicationDbContext _context;

        public AusenciasRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ------------------ Helpers de negocio ------------------
        private static DateTime Today() => DateTime.Today; 
        private static bool IsFutureDate(DateTime date) => date.Date > Today();

        private static OperationError Err(string code, string message, string? field = null)
            => new(code, message, field);

        private static void Normalize(Ausencias e)
        {
            e.FechaAusencia = e.FechaAusencia.Date;
            e.DiasAusencia = 1;
        }

        private async Task<bool> ExistsCoreAsync(string usuarioId, DateTime fechaSolo, CancellationToken ct)
        {
            var next = fechaSolo.AddDays(1);
            return await _context.Ausencias
                .AsNoTracking()
                .AnyAsync(a =>
                    a.UsuarioId == usuarioId &&
                    a.FechaAusencia >= fechaSolo &&
                    a.FechaAusencia < next, ct);
        }

        // ------------------ Create / Add ------------------
        public async Task<OperationResult<Ausencias>> CreateAsync(Ausencias entity, CancellationToken ct = default)
        {
            if (entity is null)
                return OperationResult<Ausencias>.Fail(Err("ARG_NULL", "La entidad no puede ser nula."));

            if (string.IsNullOrWhiteSpace(entity.UsuarioId))
                return OperationResult<Ausencias>.Fail(Err("USUARIO_REQUERIDO", "UsuarioId es obligatorio.", "UsuarioId"));

            Normalize(entity);

            // Regla: fecha futura (mínimo 1 día de anticipación)
            if (!IsFutureDate(entity.FechaAusencia))
                return OperationResult<Ausencias>.Fail(Err("FECHA_NO_FUTURA", "La FechaAusencia debe ser superior a la fecha actual (mínimo 1 día de anticipación).", "FechaAusencia"));

            // Regla: no duplicar por usuario + fecha (solo fecha)
            if (await ExistsCoreAsync(entity.UsuarioId!, entity.FechaAusencia, ct))
                return OperationResult<Ausencias>.Fail(Err("DUPLICADO", $"Ya existe una ausencia para {entity.UsuarioId} en {entity.FechaAusencia:yyyy-MM-dd}.", "FechaAusencia"));

            await _context.Ausencias.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return OperationResult<Ausencias>.Ok(entity, "Ausencia creada.");
        }

        public Task<OperationResult<Ausencias>> AddAsync(Ausencias entity, CancellationToken ct = default)
            => CreateAsync(entity, ct); // alias

        // ------------------ Update ------------------
        public async Task<OperationResult<Ausencias>> UpdateAsync(Ausencias entity, CancellationToken ct = default)
        {
            if (entity is null)
                return OperationResult<Ausencias>.Fail(Err("ARG_NULL", "La entidad no puede ser nula."));

            var current = await _context.Ausencias.FirstOrDefaultAsync(a => a.Id == entity.Id, ct);
            if (current is null)
                return OperationResult<Ausencias>.Fail(Err("NO_ENCONTRADO", $"No existe ausencia con Id={entity.Id}."));

             if (!IsFutureDate(entity.FechaAusencia))
                return OperationResult<Ausencias>.Fail(Err("FECHA_NO_FUTURA", "La FechaAusencia debe ser superior a la fecha actual para poder actualizar.", "FechaAusencia"));

            current.UsuarioId = entity.UsuarioId;
            current.MotivoAusencia = entity.MotivoAusencia;

            // Reglas fijas
            current.FechaAusencia = entity.FechaAusencia.Date;
            current.DiasAusencia = 1;

            _context.Ausencias.Update(current);
            await _context.SaveChangesAsync(ct);

            return OperationResult<Ausencias>.Ok(current, "Ausencia actualizada.");
        }

        // ------------------ Queries ------------------
        public async Task<OperationResult<IReadOnlyList<Ausencias>>> GetAllAsync(CancellationToken ct = default)
        {
            var data = await _context.Ausencias
                .AsNoTracking()
                .OrderByDescending(a => a.FechaAusencia)
                .ThenBy(a => a.UsuarioId)
                .ToListAsync(ct);

            return OperationResult<IReadOnlyList<Ausencias>>.Ok(data);
        }

        public async Task<OperationResult<IReadOnlyList<Ausencias>>> GetAllByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return OperationResult<IReadOnlyList<Ausencias>>.Fail(Err("USUARIO_REQUERIDO", "UsuarioId es obligatorio.", "UsuarioId"));

            var data = await _context.Ausencias
                .Where(a => a.UsuarioId == usuarioId)
                .AsNoTracking()
                .OrderByDescending(a => a.FechaAusencia)
                .ToListAsync(ct);

            return OperationResult<IReadOnlyList<Ausencias>>.Ok(data);
        }

        public async Task<OperationResult<bool>> ExistsByFechaAndUsuarioIdAsync(string usuarioId, DateTime fecha, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return OperationResult<bool>.Fail(Err("USUARIO_REQUERIDO", "UsuarioId es obligatorio.", "UsuarioId"));

            var exists = await ExistsCoreAsync(usuarioId, fecha.Date, ct);
            return OperationResult<bool>.Ok(exists);
        }

        // ------------------ Delete (hard) ------------------
        public async Task<OperationResult<bool>> DeleteByIdAsync(long id, CancellationToken ct = default)
        {
            var entity = await _context.Ausencias
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);

            if (entity is null)
                return OperationResult<bool>.Fail(Err("NO_ENCONTRADO", $"No existe ausencia con Id={id}."));

            // Regla: solo eliminar si la fecha es futura (mín. 1 día)
            if (!IsFutureDate(entity.FechaAusencia))
                return OperationResult<bool>.Fail(Err("FECHA_NO_FUTURA", "Solo se puede eliminar si la FechaAusencia es superior a la fecha actual (mínimo 1 día de anticipación).", "FechaAusencia"));

            var affected = await _context.Ausencias
                .Where(a => a.Id == id)
                .ExecuteDeleteAsync(ct);

            return OperationResult<bool>.Ok(affected > 0, affected > 0 ? "Ausencia eliminada." : "No se eliminó ninguna fila.");
        }
    }
}
