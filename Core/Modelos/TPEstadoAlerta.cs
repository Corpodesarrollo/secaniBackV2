using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class TPEstadoAlerta
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
