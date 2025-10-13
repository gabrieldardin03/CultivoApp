using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CultivoApp.Services;

namespace CultivoApp.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Mushroom> Mushrooms => AppDataService.Instance.Mushrooms;

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
            if (Mushrooms.Count > 0)
                SelectedMushroom = Mushrooms[0];

            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                if (SelectedMushroom == null || string.IsNullOrWhiteSpace(SelectedMushroom.ImagePath))
                    return;

                var uri = new Uri($"avares://CultivoApp/{SelectedMushroom.ImagePath}");
                using var stream = AssetLoader.Open(uri);
                MushroomImage = new Bitmap(stream);
            }
            catch
            {
                MushroomImage = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
