using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CultivoApp.ViewModels
{
    public class ManualViewModel : INotifyPropertyChanged
    {
        // ✅ HttpClient estático compartilhado com timeout curto
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(4)
        };

        private readonly string _ipDoESP = "192.168.1.10";

        // ✅ Debounce: cancela envio anterior se o usuário clicar rápido
        private CancellationTokenSource? _debounceCts;

        private bool _modoAutomatico;
        public bool ModoAutomatico
        {
            get => _modoAutomatico;
            set
            {
                if (_modoAutomatico != value)
                {
                    _modoAutomatico = value;
                    OnPropertyChanged();
                    AgendarEnvio();
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
                    OnPropertyChanged();
                    AgendarEnvio();
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
                    OnPropertyChanged();
                    AgendarEnvio();
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
                    OnPropertyChanged();
                    AgendarEnvio();
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

        // ✅ Debounce: espera 300ms antes de enviar
        // Se o usuário clicar de novo nesse intervalo, cancela e recomeça
        private void AgendarEnvio()
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(300, token);
                    if (!token.IsCancellationRequested)
                        await EnviarTodosEstadosAsync();
                }
                catch (TaskCanceledException) { }
            });
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
                var response = await _httpClient.GetStringAsync($"http://{_ipDoESP}/getData");
                var data = JsonSerializer.Deserialize<SensorData>(response);

                if (data != null && (data.temperatura > 0 || data.umidade > 0))
                {
                    // ✅ Só atualiza se os valores forem válidos (não zera acidentalmente)
                    Temperatura = $"{data.temperatura:F1} °C";
                    Umidade = $"{data.umidade:F1} %";
                    Status = "✅ Comunicação ativa com ESP32";
                }
            }
            catch (TaskCanceledException)
            {
                Status = "⏱ ESP32 sem resposta (timeout)";
            }
            catch
            {
                Status = "⚠️ Falha ao atualizar dados do ESP32.";
            }
        }

        // ✅ Rota correta /updateConfig com todos os campos que o ESP32 espera
        private async Task EnviarTodosEstadosAsync()
        {
            try
            {
                var payload = new
                {
                    modoAutomatico = _modoAutomatico,
                    umidificador   = _umidificadorLigado,
                    ventilador     = _aquecedorLigado,   // ESP32 chama o aquecedor de "ventilador"
                    exaustor       = _exaustorLigado
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"http://{_ipDoESP}/updateConfig", content);

                Status = response.IsSuccessStatusCode
                    ? "✅ Comando enviado com sucesso."
                    : $"⚠️ Falha ao enviar comando ({(int)response.StatusCode}).";
            }
            catch (TaskCanceledException)
            {
                Status = "⏱ Timeout ao enviar comando.";
            }
            catch (Exception ex)
            {
                Status = $"⚠️ Erro: {ex.Message}";
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