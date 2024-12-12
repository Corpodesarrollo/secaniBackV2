using Core.Interfaces.Services;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Services.MSTablasParametricas;
using Core.Services.Reportes;

namespace Core.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly Client _clienteServicio;
        private readonly TablaParametricaService _tpService;
        private const string ApiKey = "fa90576d-05e9-48ad-8bcf-30371984a699";
        private readonly IReporteDinamicoNNAService _nnaService;

        public PersonaService(Client clienteServicio, TablaParametricaService tpService, IReporteDinamicoNNAService nnaService)
        {
            _clienteServicio = clienteServicio;
            _tpService = tpService;
            _nnaService = nnaService;
        }

        public async Task<object> GetIdVigenteAsync(string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion = null)
        {
            try
            {
                return fechaExpedicion == null
                    ? await _clienteServicio.GetIdVigenteAsync(ApiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdVigente2Async(ApiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);
            }
            catch (ApiException ex)
            {
                throw new Exception($"Error en GetIdVigente: {ex.Response}", ex);
            }
        }

        public async Task<object> GetIdAllAsync(string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion = null)
        {
            try
            {
                return fechaExpedicion == null
                    ? await _clienteServicio.GetIdAllAsync(ApiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdAll2Async(ApiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);
            }
            catch (ApiException ex)
            {
                throw new Exception($"Error en GetIdAll: {ex.Response}", ex);
            }
        }

        public async Task<object> GetTipoIdentificacionVigenteAsync(string nroIdentificacion)
        {
            try
            {
                var tipoIdentificacionList = await _tpService.GetTipoIdentificaciones(default);

                foreach (var tipoIdentificacion in tipoIdentificacionList)
                {
                    var resultado = await _clienteServicio.GetIdVigenteAsync(ApiKey, tipoIdentificacion.Codigo, nroIdentificacion);

                    if (resultado != null)
                    {
                        return tipoIdentificacion.Codigo;
                    }
                }

                throw new Exception("No se encontró identificación vigente.");
            }
            catch (ApiException ex)
            {
                throw new Exception($"Error en GetTipoIdentificacionVigente: {ex.Response}", ex);
            }
        }
    }
}
