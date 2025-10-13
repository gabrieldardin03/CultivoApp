using CommunityToolkit.Mvvm.ComponentModel;

namespace CultivoApp.ViewModels
{
    public partial class ManualViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isAutomatic = true; // começa no modo automático

        [ObservableProperty]
        private bool humidifierOn;

        [ObservableProperty]
        private bool fanHeaterOn;

        [ObservableProperty]
        private bool exhaustOn;

        [ObservableProperty]
        private double temperature = 24.3;

        [ObservableProperty]
        private double humidity = 78.5;

        public bool IsManual => !IsAutomatic;

        partial void OnIsAutomaticChanged(bool value)
        {
            OnPropertyChanged(nameof(IsManual));
        }
    }
}
 