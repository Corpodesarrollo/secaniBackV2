using Core.Modelos.Common;
using System.Text.Json;

namespace Core.Services.MSTablasParametricas
{
    public class TablaParametricaService
    {
        private readonly HttpClient _httpClient;

        private readonly string _baseUrl = "https://web.sispro.gov.co/directoriogeneral/api/";

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
    }
}