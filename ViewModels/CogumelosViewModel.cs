using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CultivoApp.Services;

namespace CultivoApp.ViewModels
{
    public partial class CogumelosViewModel : ObservableObject
    {
        public ObservableCollection<Mushroom> Mushrooms => AppDataService.Instance.Mushrooms;

        [ObservableProperty]
        private Mushroom? selectedMushroom;

        [ObservableProperty]
        private bool isEditing;

        public bool IsReadOnly => !IsEditing;

        partial void OnIsEditingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsReadOnly));
        }

        [RelayCommand]
        private void AddMushroom()
        {
            var novo = new Mushroom
            {
                Name = "Novo Cogumelo",
                Temperature = "20–25 °C",
                Humidity = "80–90%",
                ImagePath = "Assets/default.png"
            };

            Mushrooms.Add(novo);
            SelectedMushroom = novo;
            IsEditing = true;
        }

        [RelayCommand]
        private void EditMushroom(Mushroom? mushroom)
        {
            if (mushroom == null) return;
            SelectedMushroom = mushroom;
            IsEditing = true;
        }

        [RelayCommand]
        private void SaveChanges()
        {
            IsEditing = false;
        }
    }
}
