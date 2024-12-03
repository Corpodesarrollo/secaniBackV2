using Core.Modelos.Common;

namespace Core.Modelos
{
    public class ReportesSIVIGILA : BaseEntity
    {
        public string? TipoIdentificacionId { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? SexoId { get; set; }
        public bool? TieneDiagnostico { get; set; }
        public int? Aseguradora { get; set; }
        public string? DepartamentoProcedenciaId { get; set; }
        public string? MunicipioProcedenciaId { get; set; }
    }
}
