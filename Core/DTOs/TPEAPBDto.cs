namespace Core.DTOs
{
    public class TPEAPBDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public long? NIT { get; set; }
        public int? DV { get; set; }
        public DateTime? Creation { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int? Tipo { get; set; }
    }
}
