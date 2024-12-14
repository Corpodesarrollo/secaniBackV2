using System.Text.Json;

namespace Infra.Helpers
{
    public static class JsonHelper
    {
        public static string ConvertToJson<T>(T obj) where T : class
        {
            // Serializa el objeto a JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Para un JSON más legible
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Usa camelCase para nombres de propiedades
            };

            return JsonSerializer.Serialize(obj, options);
        }
    }
}
