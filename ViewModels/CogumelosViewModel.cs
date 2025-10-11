using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CultivoApp.ViewModels
{
    public class CogumelosViewModel : INotifyPropertyChanged
    {
        private string _selectedMushroom = "Shimeji Marrom";
        public string SelectedMushroom
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

        public ObservableCollection<string> Mushrooms { get; } = new()
        {
            "Shimeji Marrom",
            "Shimeji Rosa",
            "Champignon",
            "Ostra",
            "Portobello"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
