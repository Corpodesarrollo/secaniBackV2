using Core.Response;

namespace Core.Modelos.Common
{
    public static class GenericRespuestaResponse
    {
        public static RespuestaResponse<TSource> Response<TSource>(bool success, string descripcion, TSource tDto)
        {
            RespuestaResponse<TSource> response = new();
            response.Estado = success;
            response.Descripcion = descripcion;
            response.Datos = tDto;
            return response;
        }

        public static RespuestaResponse<TSource> ResponseAll<TSource>(bool success, string descripcion, TSource tDto)
        {
            RespuestaResponse<TSource> response = new();
            response.Estado = success;
            response.Descripcion = descripcion;
            response.Datos = tDto;
            return response;
        }
    }
}