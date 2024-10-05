using Core.Modelos.TablasParametricas;

namespace Core.DTOs.MSTablasParametricas
{
    public class TablaParametricaDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string? TablaPadre { get; set; }
        public FuenteTabla FuenteTabla { get; set; }
    }
}
