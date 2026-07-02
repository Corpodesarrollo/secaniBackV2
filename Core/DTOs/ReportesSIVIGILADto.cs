using Core.Request;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class ReportesSIVIGILADto
    {
        public long Id { get; set; }

        [Required]
        public string? TipoIdentificacionId { get; set; }

        [Required]
        public string? NumeroIdentificacion { get; set; }

        [Required]
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }

        [Required]
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        public string? SexoId { get; set; }

        [Required]
        public bool TieneDiagnostico { get; set; }

        [Required]
        public int Aseguradora { get; set; }
        public string? DepartamentoProcedenciaId { get; set; }
        public string? MunicipioProcedenciaId { get; set; }

        // Archivos adjuntos
        public UploadFileRequest? EvidenciaDiagnostico { get; set; }
        public UploadFileRequest? EvidenciaParentesco { get; set; }
        public string? UsuarioId { get; set; }
    }
}
