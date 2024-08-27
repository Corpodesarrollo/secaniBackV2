using Core.Modelos.Common;
using System.Text.Json;

namespace Core.Services.MSTablasParametricas
{
    public class TablaParametricaService
    {
        private readonly HttpClient _httpClient;

        private readonly string _baseUrl = "https://web.sispro.gov.co/directoriogeneral/api/";
        private readonly string _baseUrlMunicipios = "https://web.sispro.gov.co/directoriogeneral/api/Municipio";
        private readonly string _baseUrlEntidades = "https://web.sispro.gov.co/directoriogeneral/api/CodigoEAPByNit";

        public TablaParametricaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<TPExternalEntityBase>> GetBynomTREF(string nomTREF, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_baseUrl + nomTREF, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonDocument.Parse(responseBody);
            var items = result.RootElement.GetProperty("items");

            var entities = new List<TPExternalEntityBase>();
            foreach (var item in items.EnumerateArray())
            {
                entities.Add(new TPExternalEntityBase
                {
                    Codigo = item.GetProperty("codigo").GetString(),
                    Nombre = item.GetProperty("nombre").GetString(),
                    Descripcion = item.GetProperty("descripcion").GetString()
                });
            }

            return entities;
        }

        public async Task<List<TPExternalEntityBase>> GetBynomTREFCodigo(string nomTREF, string Codigo, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_baseUrl + nomTREF + "/" + Codigo, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonDocument.Parse(responseBody);
            var items = result.RootElement.GetProperty("items");

            var entities = new List<TPExternalEntityBase>();
            foreach (var item in items.EnumerateArray())
            {
                entities.Add(new TPExternalEntityBase
                {
                    Codigo = item.GetProperty("codigo").GetString(),
                    Nombre = item.GetProperty("nombre").GetString(),
                    Descripcion = item.GetProperty("descripcion").GetString()
                });
            }

            return entities;
        }

        public async Task<List<TPExternalEntityBase>> GetMunicipiosByDepto(string CodigoDepto, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_baseUrlMunicipios, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonDocument.Parse(responseBody);
            var items = result.RootElement.GetProperty("items");

            var entities = new List<TPExternalEntityBase>();
            foreach (var item in items.EnumerateArray())
            {
                if (item.GetProperty("codigo").GetString().StartsWith(CodigoDepto))
                {
                    entities.Add(new TPExternalEntityBase
                    {
                        Codigo = item.GetProperty("codigo").GetString(),
                        Nombre = item.GetProperty("nombre").GetString(),
                        Descripcion = item.GetProperty("descripcion").GetString()
                    });
                }
            }

            return entities;
        }

        public async Task<List<TPEntidadExterna>> GetEntidates(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_baseUrlEntidades, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonDocument.Parse(responseBody);
            var items = result.RootElement.GetProperty("items");

            var entities = new List<TPEntidadExterna>();
            foreach (var item in items.EnumerateArray())
            {
                entities.Add(new TPEntidadExterna
                {
                    Codigo = item.GetProperty("codigo").GetString(),
                    Nombre = item.GetProperty("nombre").GetString(),
                    Descripcion = item.GetProperty("descripcion").GetString(),
                    NITConCode = item.GetProperty("extra_V").GetString(),
                    NITSinCode = item.GetProperty("extra_III").GetString(),
                    DigitoVerificacion = item.GetProperty("extra_IV").GetString(),
                    CategoriaVIII = item.GetProperty("extra_VIII").GetString(),
                    CategoriaIX = item.GetProperty("extra_IX").GetString(),
                    Email = item.GetProperty("extra_X").GetString(),
                });
            }

            return entities;
        }

        public async Task<TPEntidadExterna> GetEntidadById(string CodigoEntidad, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(_baseUrlEntidades + "/" + CodigoEntidad, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonDocument.Parse(responseBody);
            var items = result.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0)
            {
                return null;
            }
            var item = items.EnumerateArray().FirstOrDefault();

            var entidad = new TPEntidadExterna()
            {
                Codigo = item.GetProperty("codigo").GetString(),
                Nombre = item.GetProperty("nombre").GetString(),
                Descripcion = item.GetProperty("descripcion").GetString(),
                NITConCode = item.GetProperty("extra_V").GetString(),
                NITSinCode = item.GetProperty("extra_III").GetString(),
                DigitoVerificacion = item.GetProperty("extra_IV").GetString(),
                CategoriaVIII = item.GetProperty("extra_VIII").GetString(),
                CategoriaIX = item.GetProperty("extra_IX").GetString(),
                Email = item.GetProperty("extra_X").GetString(),
            };

            return entidad;
        }
    }
}