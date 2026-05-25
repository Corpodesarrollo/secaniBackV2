namespace Core.DTOs
{
    public class SeguimientoDto
    {
        public long Id { get; set; }
        public long? NoCaso { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? NombreCompleto
        {
            get
            {
                return $"{PrimerNombre} {SegundoNombre} {PrimerApellido} {SegundoApellido}";
            }
        }
        public string? TipoIdentificacion { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? Sexo { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public string? Parentesco { get; set; }
        public string? Diagnostico { get; set; }
        public string? Aseguradora { get; set; }
        public string? EstadoSeguimiento { get; set; }
        public TPEstadoNNADto? Estado { get; set; }
        public string? AsuntoUltimaActuacion { get; set; }

        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaUltimaActuacion { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public List<AlertaSeguimientoDto>? Alertas { get; set; }
        public string? Observaciones { get; set; }
        public string? EntidadAlerta { get; set; }
        public string? UsuarioId { get; set; }
        public string? Usuario { get; set; }
    }
}
