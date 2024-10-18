using Core.Interfaces.Repositorios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repositorios
{
    public class GeneralCOM : IGeneralCOM
    {
        private readonly HttpClient _httpClient;

        public GeneralCOM()
        {
            _httpClient = new HttpClient();
        }

        public async Task AsignarSeguimientoAutomatico(string requestUrl)
        {
            var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead);
            var data = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
                throw new Exception(data);
        }
    }
}
