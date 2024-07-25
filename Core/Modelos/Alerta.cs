using Core.Modelos.Common;

namespace Core.Modelos
{
    public class Alerta : BaseEntity
    {
        public int SubcategoriaId { get; set; }
        public string Descripcion { get; set; }
        public char Alias { get; set; }
    }
}
