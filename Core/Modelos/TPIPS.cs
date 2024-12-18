namespace Core.Modelos
{
    public class TPIPS
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public bool Habilitado { get; set; }
        public string? CodigoMunicipio { get; set; }
        public string? NombreMunicipio { get; set; }
        public DateTime? Creation { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
