using Core.Modelos.TablasParametricas;
using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace Core.DTOs.MSTablasParametricas
{
    public class TablaParametricaDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string? TablaPadre { get; set; }
        public string? NombreTablaPadre { get; set; }
        public FuenteTabla FuenteTabla { get; set; }
    }
}
