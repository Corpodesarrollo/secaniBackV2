namespace Core.Modelos.TablasParametricas
{
    public enum FuenteTabla
    {
        SISPRO,
        Local
    }
    public class TablaParametrica
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string? TablaPadre { get; set; } = null;
        public FuenteTabla FuenteTabla { get; set; }
    }
}
