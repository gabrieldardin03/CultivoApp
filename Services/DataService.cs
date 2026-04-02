using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CultivoApp.Services
{
    public static class DataService
    {
        private static readonly HttpClient client = new HttpClient();
        private static bool isRunning = false;

        public static async Task StartUpdatingDataAsync()
        {
            if (isRunning) return;
            isRunning = true;

            while (true)
            {
                try
                {
                    string response = await client.GetStringAsync($"{AppData.EspIpAddress}/getData");
                    var data = JsonSerializer.Deserialize<SensorData>(response);

                    if (data != null)
                    {
                        AppData.Temperature = data.Temperature;
                        AppData.Humidity = data.Humidity;
                    }
                }
                catch
                {
                    // tenta reconectar sozinho a cada 5 segundos
                }

                await Task.Delay(5000);
            }
        }

        public static async Task SendManualCommandAsync(string action)
        {
            try
            {
                await client.GetAsync($"{AppData.EspIpAddress}/{action}");
            }
            catch { }
        }

        private class SensorData
        {
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}
