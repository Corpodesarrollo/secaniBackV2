using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Adjuntos : BaseEntity
    {
        public string? Descripcion { get; set; }
        public string? Url { get; set; }
        public string? NombreArchivo { get; set; }
    }
}
