using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace EstufaInteligente.Services
{
    public class SensorService
    {
        private readonly HttpClient _http = new();
        private readonly Timer _timer;
        private const string EspUrl = "http://192.168.1.200/getData";

        public float Temperatura { get; private set; }
        public float Umidade { get; private set; }

        public event Action? DadosAtualizados;

        public SensorService()
        {
            _timer = new Timer(10000); // 10 segundos
            _timer.Elapsed += async (s, e) => await AtualizarDadosAsync();
            _timer.AutoReset = true;
        }

        public void Iniciar() => _timer.Start();

        private async Task AtualizarDadosAsync()
        {
            try
            {
                string json = await _http.GetStringAsync(EspUrl);
                using var doc = JsonDocument.Parse(json);

                Temperatura = doc.RootElement.GetProperty("temperatura").GetSingle();
                Umidade = doc.RootElement.GetProperty("umidade").GetSingle();

                DadosAtualizados?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar dados: {ex.Message}");
            }
        }
    }
}
