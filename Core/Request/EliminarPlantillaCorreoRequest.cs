namespace Core.Request
{
    public class EliminarPlantillaCorreoRequest
    {
        public long Id {  get; set; }
        public string Comentario { get; set; }
        public string IdUsuario { get; set; }
        public string Rol { get; set; }
    }
}
