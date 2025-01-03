using Core.Modelos;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infra.Middleware
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly string _userId;

        public AuditInterceptor()
        {
        }

        private void AddAuditEntries(DbContext context)
        {
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    // Intentar obtener el UserId del registro
                    string userId = null;
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        userId = entry.CurrentValues.Properties
                            .FirstOrDefault(p => p.Name == "UserId") != null
                                ? entry.CurrentValues["UserId"]?.ToString()
                                : null;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        userId = entry.OriginalValues.Properties
                            .FirstOrDefault(p => p.Name == "UserId") != null
                                ? entry.OriginalValues["UserId"]?.ToString()
                                : null;
                    }

                    // Crear el registro de auditoría
                    var auditEntry = new AuditEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId ?? "Unknown", // Manejar caso cuando UserId no esté disponible
                        FechaTransaccion = DateTime.UtcNow,
                        Tabla = entry.Entity.GetType().Name,
                        Operacion = entry.State.ToString(),
                        RegistroAnterior = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                            ? JsonSerializer.Serialize(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]))
                            : null,
                        RegistroNuevo = entry.State == EntityState.Added || entry.State == EntityState.Modified
                            ? JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]))
                            : null
                    };

                    auditEntries.Add(auditEntry);
                }
            }

            if (auditEntries.Any())
            {
                context.Set<AuditEntry>().AddRange(auditEntries);
            }
        }



        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;

            if (context != null)
            {
                AddAuditEntries(context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            if (context != null)
            {
                AddAuditEntries(context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

}
