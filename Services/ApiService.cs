using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CultivoApp.Services
{
    public static class ApiService
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5) // Timeout ajustado para 5 segundos
        };
        private static readonly string baseUrl = "http://192.168.1.10"; // IP do ESP32

        // Retorna a temperatura, umidade e os estados dos relés (para salvar no histórico)
        public static async Task<(double temperatura, double umidade, bool modoAuto, bool aquecedor, bool umidificador, bool exaustor)> GetDadosCompletosAsync()
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}/getData");
                if (!response.IsSuccessStatusCode) return (double.NaN, double.NaN, true, false, false, false);

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var temp = root.GetProperty("temperatura").GetDouble();
                var umid = root.GetProperty("umidade").GetDouble();
                
                // Lendo os estados dos relés para espelhar o log no C#
                bool modoAuto = root.GetProperty("modoAutomatico").GetBoolean();
                bool aquecedor = root.GetProperty("ventiladorLigado").GetBoolean();
                bool umidificador = root.GetProperty("umidificadorLigado").GetBoolean();
                bool exaustor = root.GetProperty("exaustorLigado").GetBoolean();

                return (temp, umid, modoAuto, aquecedor, umidificador, exaustor);
            }
            catch
            {
                return (double.NaN, double.NaN, true, false, false, false);
            }
        }

        public static async Task<bool> UpdateConfigAsync(string cultura, double tempMin, double tempMax, double umidMin, double umidMax)
        {
            // Chaves corrigidas sem o underline para casar com o ESP32
            var data = new
            {
                cultura = cultura,
                tempMin = tempMin,
                tempMax = tempMax,
                umidMin = umidMin,
                umidMax = umidMax,
                modoAutomatico = true
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

        // Função para puxar o log de erros completo direto do Cartão SD do ESP32
        public static async Task<string> DownloadLogEsp32Async()
        {
            try
            {
                var response = await client.GetAsync($"{baseUrl}/downloadLog");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return $"Erro na resposta do ESP32: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Falha ao conectar com o ESP32 para baixar o log.\n{ex.Message}";
            }
        }
    }
}