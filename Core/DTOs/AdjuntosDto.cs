using Core.Modelos;

namespace Core.DTOs
{
    public class AdjuntosDto
    {
        public long Id { get; set; }
        public string? NombreArchivo { get; set; }
        public string? Descripcion { get; set; }
        public string? Url { get; set; }
        public long? Referencia { get; set; }
        public TipoAdjunto Tipo { get; set; }
    }
}
