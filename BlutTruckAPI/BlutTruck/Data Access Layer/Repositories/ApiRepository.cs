using BlutTruck.Data_Access_Layer.IRepositories;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BlutTruck.Data_Access_Layer.Repositories
{
    public class ApiRepository : IApiRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _PredictBaseUrl;

        public ApiRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _PredictBaseUrl = configuration["ApiSettings:PredictBaseUrl"];
        }

        public async Task<string> PredecirAsync(Dictionary<string, object> datos)
        {
            try
            {
                // Construir la URL completa
                string url = $"{_PredictBaseUrl}/predecir";

                // Realizar POST con los datos
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, datos);

                // Verificar la respuesta
                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<JsonDocument>();
                    return resultado?.RootElement.GetProperty("prediccion").ToString();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error en la API: {error}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el repositorio: {ex.Message}");
            }
        }
    }
}
