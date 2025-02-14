using System.ComponentModel.DataAnnotations;

namespace Core.Modelos
{
    public class TPEAPB
    {
        [Key]
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public long? NIT { get; set; }
        public int? DV { get; set; }
        public DateTime? Creation { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
