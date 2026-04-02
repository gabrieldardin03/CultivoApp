using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CultivoApp.Services
{
    public static class ApiService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string baseUrl = "http://192.168.1.200"; // ⬅️ coloque aqui o IP do ESP32 mostrado no monitor serial

        public static async Task<(double temperatura, double umidade)> GetDadosAsync()
        {
            try
            {
                var response = await client.GetStringAsync($"{baseUrl}/getData");
                using var doc = JsonDocument.Parse(response);
                var temp = doc.RootElement.GetProperty("temperatura").GetDouble();
                var umid = doc.RootElement.GetProperty("umidade").GetDouble();
                return (temp, umid);
            }
            catch
            {
                return (double.NaN, double.NaN);
            }
        }

        public static async Task<bool> UpdateConfigAsync(double tempMin, double tempMax, double umidMin, double umidMax)
        {
            var data = new
            {
                temp_min = tempMin,
                temp_max = tempMax,
                umid_min = umidMin,
                umid_max = umidMax
            };
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}/updateConfig", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
