﻿namespace Core.DTOs
{
    public class ContactoEntidadRequest
    {
        public long EntidadId { get; set; }
        public string Nombres { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public string Telefonos { get; set; }
    }
}
