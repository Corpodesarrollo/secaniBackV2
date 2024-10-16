﻿using Core.Request;
using Core.response;

namespace Core.Interfaces.Repositorios
{
    public interface INotificacionRepo
    {
        public List<GetNotificacionResponse> GetNotificacionUsuario(string AgenteDestinoId);
        public int GetNumeroNotificacionUsuario(string AgenteDestinoId);
        public string GenerarOficioNotificacion(OficioNotificacionRequest request);
        public void EliminarNotificacion(EliminarNotificacionRequest request);
    }
}
