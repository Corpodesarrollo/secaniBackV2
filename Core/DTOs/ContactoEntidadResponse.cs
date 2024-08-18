﻿using Core.Modelos;
using Core.Modelos.Common;

namespace Core.DTOs
{
    public class ContactoEntidadResponse : BaseEntity
    {
        public string EntidadId { get; set; }
        public string Nombres { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
    }
}
