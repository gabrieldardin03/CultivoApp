using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CultivoApp.ViewModels
{
    public class ManualViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new();
        private readonly string ipDoESP = "192.168.1.200"; // troque conforme o IP real do ESP32

        private bool _modoAutomatico;
        public bool ModoAutomatico
        {
            get => _modoAutomatico;
            set
            {
                if (_modoAutomatico != value)
                {
                    _modoAutomatico = value;
                    _ = EnviarComandoAsync("modoAutomatico", value);
                    OnPropertyChanged();
                }
            }
        }

        private bool _umidificadorLigado;
        public bool UmidificadorLigado
        {
            get => _umidificadorLigado;
            set
            {
                if (_umidificadorLigado != value)
                {
                    _umidificadorLigado = value;
                    _ = EnviarComandoAsync("umidificador", value);
                    OnPropertyChanged();
                }
            }
        }

        private bool _exaustorLigado;
        public bool ExaustorLigado
        {
            get => _exaustorLigado;
            set
            {
                if (_exaustorLigado != value)
                {
                    _exaustorLigado = value;
                    _ = EnviarComandoAsync("exaustor", value);
                    OnPropertyChanged();
                }
            }
        }

        private bool _aquecedorLigado;
        public bool AquecedorLigado
        {
            get => _aquecedorLigado;
            set
            {
                if (_aquecedorLigado != value)
                {
                    _aquecedorLigado = value;
                    _ = EnviarComandoAsync("aquecedor", value);
                    OnPropertyChanged();
                }
            }
        }

        private string _temperatura = "--";
        public string Temperatura
        {
            get => _temperatura;
            set { _temperatura = value; OnPropertyChanged(); }
        }

        private string _umidade = "--";
        public string Umidade
        {
            get => _umidade;
            set { _umidade = value; OnPropertyChanged(); }
        }

        private string _status = "Aguardando conexão com ESP32...";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public ManualViewModel()
        {
            _ = AtualizarLeiturasPeriodicamente();
        }

        private async Task AtualizarLeiturasPeriodicamente()
        {
            while (true)
            {
                await AtualizarLeiturasAsync();
                await Task.Delay(5000);
            }
        }

        private async Task AtualizarLeiturasAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"http://{ipDoESP}/getData");
                var data = JsonSerializer.Deserialize<SensorData>(response);

                if (data != null)
                {
                    Temperatura = $"{data.temperatura:F1} °C";
                    Umidade = $"{data.umidade:F1} %";
                    Status = "✅ Comunicação ativa com ESP32";
                }
            }
            catch
            {
                Status = "⚠️ Falha ao atualizar dados do ESP32.";
            }
        }

        private async Task EnviarComandoAsync(string dispositivo, bool ligado)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { device = dispositivo, state = ligado });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"http://{ipDoESP}/manualControl", content);

                Status = response.IsSuccessStatusCode
                    ? $"✅ {dispositivo} {(ligado ? "ligado" : "desligado")}."
                    : $"⚠️ Falha ao enviar comando.";
            }
            catch
            {
                Status = "⚠️ Não foi possível se comunicar com o ESP32.";
            }
        }

        private class SensorData
        {
            public double temperatura { get; set; }
            public double umidade { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
