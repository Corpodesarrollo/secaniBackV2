namespace Core.DTOs
{
    // BUG QA 2026-05-25: DTO minimo para listados (dropdowns) donde solo se necesitan
    // id/codigo/nombre. Reduce payload del endpoint GET /IPS (~128k items) de ~28 MB
    // raw / 3 MB gzip a ~10 MB / 1 MB gzip al omitir Habilitado, Municipio*, Creation,
    // LastUpdate que el frontend no usa para seleccionar.
    public class TPIPSSlimDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
    }
}
