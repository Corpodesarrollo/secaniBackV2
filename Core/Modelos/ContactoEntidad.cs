﻿using Core.Modelos.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Core.Modelos
{
    public class ContactoEntidad : BaseEntity
    {
        public string EntidadId { get; set; }
        public string Nombres { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
        public bool? Activo { get; set; }
        public string ActivoName { get; set; }
    }

    public class ContactoEntidadConfiguration : IEntityTypeConfiguration<ContactoEntidad>
    {
        public void Configure(EntityTypeBuilder<ContactoEntidad> builder)
        {

        }
    }
}
