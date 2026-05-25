namespace Core.DTOs
{
    public class RespuestaSisproDto
    {
        public string? Tref { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Habilitado { get; set; }
        public DateTime? LastUpdate { get; set; }
        public ItemDto[]? Items { get; set; }
    }
}
