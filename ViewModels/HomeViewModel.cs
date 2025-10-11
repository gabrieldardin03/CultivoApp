using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace CultivoApp.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Mushroom> Mushrooms { get; }

        private Mushroom _selectedMushroom = null!;
        public Mushroom SelectedMushroom
        {
            get => _selectedMushroom;
            set
            {
                if (_selectedMushroom != value)
                {
                    _selectedMushroom = value;
                    OnPropertyChanged();
                    LoadImage();
                }
            }
        }

        private Bitmap? _mushroomImage;
        public Bitmap? MushroomImage
        {
            get => _mushroomImage;
            set
            {
                _mushroomImage = value;
                OnPropertyChanged();
            }
        }

        private string _debugMessage = string.Empty;
        public string DebugMessage
        {
            get => _debugMessage;
            set
            {
                _debugMessage = value;
                OnPropertyChanged();
            }
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
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelos_shimeji_marrom.png"
                },
                new Mushroom
                {
                    Name = "Shimeji Rosa",
                    Temperature = "Temperatura Ideal: 20–24 °C",
                    Humidity = "Umidade Ideal: 80–90 %",
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelo_shimeji_rosa.png"
                },
                new Mushroom
                {
                    Name = "Champignon",
                    Temperature = "Temperatura Ideal: 18–22 °C",
                    Humidity = "Umidade Ideal: 75–85 %",
                    ImagePath = "avares://CultivoApp/Assets/Images/cogumelos_champignon.png"
                }
            };

            SelectedMushroom = Mushrooms[0];
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                var uri = new Uri(SelectedMushroom.ImagePath);
                using var stream = AssetLoader.Open(uri);
                MushroomImage = new Bitmap(stream);
                DebugMessage = "✅ Imagem carregada e exibida com sucesso!";
            }
            catch (Exception ex)
            {
                MushroomImage = null;
                DebugMessage = $"❌ Erro ao carregar imagem: {ex.Message}";
            }
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
