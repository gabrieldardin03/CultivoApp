using CommunityToolkit.Mvvm.ComponentModel;

namespace CultivoApp.ViewModels
{
    public partial class ManualViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isAutomatic = true;

        [ObservableProperty]
        private bool humidifierOn;

        [ObservableProperty]
        private bool fanHeaterOn;

        [ObservableProperty]
        private bool exhaustOn;

        [ObservableProperty]
        private double temperature = 24.5;

        [ObservableProperty]
        private double humidity = 78.2;

        // 🔹 Calcula se o modo manual está ativo
        public bool IsManual => !IsAutomatic;

        // 🔹 Notifica mudança no IsManual quando o modo automático muda
        partial void OnIsAutomaticChanged(bool value)
        {
            OnPropertyChanged(nameof(IsManual));
        }
    }
}
