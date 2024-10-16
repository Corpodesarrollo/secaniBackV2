﻿using Core.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Infra
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<ContactoEntidad>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    var entity = entry.Entity;
                    entity.DateCreated = DateTime.UtcNow;
                    entity.CreatedByUserId = "1";
                    entity.IsDeleted = false;
                }

                if (entry.State == EntityState.Modified)
                {
                    var entity = entry.Entity;
                    entity.DateUpdated = DateTime.UtcNow;
                    entity.UpdatedByUserId = "2";
                }

                if (entry.State == EntityState.Deleted)
                {
                    var entity = entry.Entity;
                    entity.DateDeleted = DateTime.UtcNow;
                    entity.DeletedByUserId = "3";
                    entity.IsDeleted = true;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la vista VwMenu como una entidad sin clave
            modelBuilder.Entity<VwMenuModel>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("VwMenu"); // Esto es válido para versiones recientes de EF Core
            });

            // Configurar la vista VwMenu como una entidad sin clave
            modelBuilder.Entity<VwSubMenuModel>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("VwSubMenu"); // Esto es válido para versiones recientes de EF Core
            });

            modelBuilder.Entity<VwAgentesAsignados>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("VwAgentesAsignados"); // Esto es válido para versiones recientes de EF Core
            });
            
            modelBuilder.Entity<NNAs>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Configuración adicional
            });

            // Configuración para `FiltroNNA`
            modelBuilder.Entity<FiltroNNA>(entity =>
            {
                entity.HasNoKey();  // Si FiltroNNA es una entidad sin clave
                                    // Configuración adicional
            });
        }

        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<AlertaSeguimiento> AlertaSeguimientos { get; set; }
        public DbSet<Notificacion> Notificacions { get; set; }
        public DbSet<Seguimiento> Seguimientos { get; set; }
        public DbSet<UsuarioAsignado> UsuarioAsignados { get; set; }
        public DbSet<NotificacionesUsuario> NotificacionesUsuarios { get; set; }
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<NotificacionEntidad> NotificacionesEntidad { get; set; }
        public DbSet<Entidad> Entidades { get; set; }
        public DbSet<NNAs> NNAs { get; set; }
        public DbSet<Intentos> Intentos { get; set; }
        public DbSet<ContactoNNA> ContactoNNAs { get; set; }
        public DbSet<ContactoEntidad> ContactoEntidades { get; set; }
        public DbSet<VwMenuModel> VwMenu { get; set; }
        public DbSet<VwSubMenuModel> VwSubMenu { get; set; }

        public DbSet<VwAgentesAsignados> VwAgentesAsignados { get; set; }
        public DbSet<FiltroNNA> FiltroNNAs { get; set; }

        public DbSet<TPEstadoNNA> TPEstadoNNA { get;set; }
    }
}
