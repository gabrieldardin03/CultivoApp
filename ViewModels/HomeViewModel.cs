using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CultivoApp.Models;
using CultivoApp.Services;

namespace CultivoApp.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Mushroom> Mushrooms => AppDataService.Instance.Mushrooms;

        // ✅ HttpClient compartilhado com timeout curto para não travar a UI
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(4)
        };

        private readonly string _ipDoESP = "192.168.1.200";

        private Mushroom _selectedMushroom = null!;
        public Mushroom SelectedMushroom
        {
            get => _selectedMushroom;
            set
            {
                if (_selectedMushroom != value)
                {
                    _selectedMushroom = value;
                    AppDataService.Instance.SaveData(value);
                    LoadImage();
                    AtualizarValoresIdeais();
                    _ = EnviarConfiguracaoParaESP(value);
                    OnPropertyChanged();
                }
            }
        }

        private Bitmap? _mushroomImage;
        public Bitmap? MushroomImage
        {
            get => _mushroomImage;
            set { _mushroomImage = value; OnPropertyChanged(); }
        }

        private string _debugMessage = "";
        public string DebugMessage
        {
            get => _debugMessage;
            set { _debugMessage = value; OnPropertyChanged(); }
        }

        public double Temperatura => AppDataService.Instance.TemperaturaAtual;
        public double Umidade => AppDataService.Instance.UmidadeAtual;
        public string UltimaLeitura => AppDataService.Instance.UltimaLeitura == DateTime.MinValue
            ? "--:--"
            : AppDataService.Instance.UltimaLeitura.ToString("HH:mm:ss");

        public double TempMinIdeal { get; set; }
        public double TempMaxIdeal { get; set; }
        public double UmidMinIdeal { get; set; }
        public double UmidMaxIdeal { get; set; }

        public HomeViewModel()
        {
            if (AppDataService.Instance.LastSelected != null)
                SelectedMushroom = AppDataService.Instance.LastSelected!;
            else if (Mushrooms.Count > 0)
                SelectedMushroom = Mushrooms[0];

            LoadImage();
            AtualizarValoresIdeais();
            AppDataService.Instance.DadosAtualizados += AtualizarTela;

            _ = MonitorarESPAsync();
        }

        private async Task MonitorarESPAsync()
        {
            while (true)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"http://{_ipDoESP}/getData");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        var temp = doc.RootElement.GetProperty("temperatura").GetDouble();
                        var umid = doc.RootElement.GetProperty("umidade").GetDouble();

                        // ✅ Só atualiza se os valores forem válidos (não zera acidentalmente)
                        if (temp > 0 || umid > 0)
                        {
                            AppDataService.Instance.AtualizarDadosSensor(temp, umid);
                            DebugMessage = $"🌡 {temp:F1}°C | 💧 {umid:F1}%";
                        }
                    }
                    else
                    {
                        DebugMessage = $"ESP32 offline ({response.StatusCode})";
                    }
                }
                catch (TaskCanceledException)
                {
                    DebugMessage = "⏱ ESP32 sem resposta (timeout)";
                }
                catch
                {
                    DebugMessage = "❌ Tentando reconectar ao ESP32...";
                }

                await Task.Delay(5000);
            }
        }

        private async Task EnviarConfiguracaoParaESP(Mushroom cogumelo)
        {
            try
            {
                var parametros = new
                {
                    tempMin = cogumelo.TempMin,
                    tempMax = cogumelo.TempMax,
                    umidMin = cogumelo.UmidMin,
                    umidMax = cogumelo.UmidMax
                };

                var json = JsonSerializer.Serialize(parametros);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"http://{_ipDoESP}/updateConfig", content);
                DebugMessage = response.IsSuccessStatusCode
                    ? "✅ Configuração enviada com sucesso."
                    : "⚠️ Falha ao enviar configuração.";
            }
            catch (TaskCanceledException)
            {
                DebugMessage = "⏱ Timeout ao enviar configuração.";
            }
            catch (Exception ex)
            {
                DebugMessage = $"Erro: {ex.Message}";
            }
        }

        private void AtualizarValoresIdeais()
        {
            if (SelectedMushroom == null) return;
            TempMinIdeal = SelectedMushroom.TempMin;
            TempMaxIdeal = SelectedMushroom.TempMax;
            UmidMinIdeal = SelectedMushroom.UmidMin;
            UmidMaxIdeal = SelectedMushroom.UmidMax;
        }

        private void LoadImage()
        {
            try
            {
                if (SelectedMushroom == null || string.IsNullOrWhiteSpace(SelectedMushroom.ImagePath))
                {
                    MushroomImage = null;
                    return;
                }

                if (File.Exists(SelectedMushroom.ImagePath))
                {
                    using var stream = File.OpenRead(SelectedMushroom.ImagePath);
                    MushroomImage = new Bitmap(stream);
                }
                else
                {
                    var uri = new Uri($"avares://CultivoApp/{SelectedMushroom.ImagePath}");
                    using var stream = AssetLoader.Open(uri);
                    MushroomImage = new Bitmap(stream);
                }
            }
            catch
            {
                MushroomImage = null;
            }
        }

        private void AtualizarTela()
        {
            OnPropertyChanged(nameof(Temperatura));
            OnPropertyChanged(nameof(Umidade));
            OnPropertyChanged(nameof(UltimaLeitura));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}