using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Seguimiento : BaseEntity
    {
        public long NNAId { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public int EstadoId { get; set; }
        public long ContactoNNAId { get; set; }
        public string? Telefono { get; set; }
        public string? UsuarioId { get; set; }
        // BUG-LZ-083: era long? pero los DTOs (SetSeguimientoRequest/GetSeguimientoResponse) y los
        // ids de AspNetUsers son string (ej. "qa-cuidador-003"). Como long nunca podia guardar el
        // id del solicitante, GetSeguimientosCuidador (long.TryParse) devolvia lista vacia.
        public string? SolicitanteId { get; set; }
        public DateTime? FechaSolicitud { get; set; }
        public bool? TieneDiagnosticos { get; set; }
        public string? ObservacionesSolicitante { get; set; }
        public string? ObservacionAgente { get; set; }
        public string? UltimaActuacionAsunto { get; set; }
        public DateTime? UltimaActuacionFecha { get; set; }
        public string? NombreRechazo { get; set; }
        public string? ParentescoRechazo { get; set; }
        public string? RazonesRechazo { get; set; }
        // Regla negocio: 1 NNA = 1 seguimiento Activo. Al crear uno nuevo, los previos quedan
        // Activo=false. Heredan alertas en AlertaSeguimientos (snapshot Design B).
        public bool Activo { get; set; } = true;
    }
}

