namespace Core.Modelos.Common
{
    public class TPEntidadExterna : TPExternalEntityBase
    {
        public string NITConCode { get; set; }
        public string NITSinCode { get; set; }
        public string DigitoVerificacion { get; set; }
        public string CategoriaVIII { get; set; }
        public string CategoriaIX { get; set; }
        public string Email { get; set; }
    }
}
