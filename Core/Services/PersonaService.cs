using Core.Interfaces.Services;
using Core.Interfaces.Services.MSUsuariosyRoles;
using Core.Interfaces.Services.Reportes;
using Core.Services.MSTablasParametricas;
using Core.Services.Reportes;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly Client _clienteServicio;
        private readonly TablaParametricaService _tpService;
        // RQ-10-HU02: ApiKey leida desde appsettings/env var (SisproApi:ApiKey)
        private readonly string _apiKey;
        private readonly IReporteDinamicoNNAService _nnaService;

        public PersonaService(Client clienteServicio, TablaParametricaService tpService, IReporteDinamicoNNAService nnaService, IConfiguration configuration)
        {
            _clienteServicio = clienteServicio;
            _tpService = tpService;
            _nnaService = nnaService;
            _apiKey = configuration["SisproApi:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Console.WriteLine("WARN: SisproApi:ApiKey vacia. Llamadas a Maestro Personas fallaran. Configurar via appsettings o env var SisproApi__ApiKey.");
            }
        }

        public async Task<object> GetIdVigenteAsync(string tipoIdentificacion, string nroIdentificacion, string fechaExpedicion = null)
        {
            try
            {
                return fechaExpedicion == null
                    ? await _clienteServicio.GetIdVigenteAsync(_apiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdVigente2Async(_apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);
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
                    ? await _clienteServicio.GetIdAllAsync(_apiKey, tipoIdentificacion, nroIdentificacion)
                    : await _clienteServicio.GetIdAll2Async(_apiKey, tipoIdentificacion, nroIdentificacion, fechaExpedicion);
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
                    var resultado = await _clienteServicio.GetIdVigenteAsync(_apiKey, tipoIdentificacion.Codigo, nroIdentificacion);

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
