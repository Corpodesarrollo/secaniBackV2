using Core.Modelos.Common;

namespace Core.Modelos
{
    public class NNAsSeguimiento : BaseEntity
    {
        public string? PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public DateTime FechaNotificacionSIVIGILA { get; set; }
    }
}
