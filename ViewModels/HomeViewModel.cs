using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Platform;

namespace CultivoApp.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Mushroom> Mushrooms { get; }

        private Mushroom _selectedMushroom;
        public Mushroom SelectedMushroom
        {
            get => _selectedMushroom;
            set
            {
                if (_selectedMushroom != value)
                {
                    _selectedMushroom = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _debugMessage = "";
        public string DebugMessage
        {
            get => _debugMessage;
            set { _debugMessage = value; OnPropertyChanged(); }
        }

        public HomeViewModel()
        {
            Mushrooms = new ObservableCollection<Mushroom>
            {
                new Mushroom
                {
                    Name = "Shimeji Marrom",
                    Temperature = "Temperatura Ideal: 22–24 °C",
                    Humidity = "Umidade Ideal: 80–90 %",
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelos_shimeji_marrom.jpg"
                },
                new Mushroom
                {
                    Name = "Shimeji Rosa",
                    Temperature = "Temperatura Ideal: 20–24 °C",
                    Humidity = "Umidade Ideal: 80–90 %",
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelo_shimeji_rosa.jpg"
                },
                new Mushroom
                {
                    Name = "Champignon",
                    Temperature = "Temperatura Ideal: 18–22 °C",
                    Humidity = "Umidade Ideal: 75–85 %",
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelos_Champignon.png"
                }
            };

            SelectedMushroom = Mushrooms[0];

            // 🔍 TESTE VISUAL NA INTERFACE
            var uriTest = new Uri(Mushrooms[0].ImagePath);
            bool exists = AssetLoader.Exists(uriTest);
            DebugMessage = exists
                ? "✅ Imagem encontrada corretamente."
                : "❌ Erro: imagem não encontrada. Caminho incorreto?";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class Mushroom
    {
        public string Name { get; set; } = string.Empty;
        public string Temperature { get; set; } = string.Empty;
        public string Humidity { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
