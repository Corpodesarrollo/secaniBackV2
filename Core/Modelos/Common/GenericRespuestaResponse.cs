using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Response;
using Microsoft.Identity.Client;

namespace Core.Modelos.Common
{
    public static class GenericRespuestaResponse
    {
        public static RespuestaResponse<TSource> Response<TSource>(bool success, string descripcion, TSource tDto)
        {
            RespuestaResponse<TSource> response = new RespuestaResponse<TSource>();
            response.Estado = success;
            response.Descripcion = descripcion;
            response.Datos = new List<TSource> { tDto };
            return response;
        }

        public static RespuestaResponse<TSource> ResponseAll<TSource>(bool success, string descripcion, List<TSource> tDto)
        {
            RespuestaResponse<TSource> response = new RespuestaResponse<TSource>();
            response.Estado = success;
            response.Descripcion = descripcion;
            response.Datos =  tDto ;
            return response;
        }
    }
}