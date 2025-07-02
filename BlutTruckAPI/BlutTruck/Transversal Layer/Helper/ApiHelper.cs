using System.Net.Http.Headers;

namespace BlutTruck.Transversal_Layer.Helper
{
    public static class ApiHelper
    {
        public static HttpClient ConfigurarHttpClient(string baseUrl)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
