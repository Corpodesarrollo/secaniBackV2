namespace Core.Response
{
    public class RespuestaResponse<T>
    {
        public bool Estado { get; set; }
        public string? Descripcion { get; set; }
        public T? Datos { get; set; }
    }
}
