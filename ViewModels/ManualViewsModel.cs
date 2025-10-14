using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace CultivoApp.ViewModels
{
    public partial class ManualViewModel : ObservableObject
    {
        // IsAutomatic (bound ao ToggleSwitch principal)
        [ObservableProperty]
        private bool isAutomatic = true;

        // quando IsAutomatic muda, notificamos que IsManual mudou também
        partial void OnIsAutomaticChanged(bool value)
        {
            // Notifica IsManual (propriedade calculada)
            OnPropertyChanged(nameof(IsManual));
            // quando volta ao automático, por segurança desligamos os manuais (opcional)
            if (value)
            {
                HumidifierOn = false;
                FanHeaterOn = false;
                ExhaustOn = false;
            }
        }

        // propriedade calculada (usada no XAML) — true quando não estiver automático
        public bool IsManual => !IsAutomatic;

        // switches individuais
        [ObservableProperty] private bool humidifierOn;
        [ObservableProperty] private bool fanHeaterOn;
        [ObservableProperty] private bool exhaustOn;

        // Simulação de leituras (você pode ligar seu serviço depois)
        [ObservableProperty] private double temperature = 24.3;
        [ObservableProperty] private double humidity = 78.2;

        // Comandos (opcionais — exemplo para testar)
        [RelayCommand]
        private void ToggleHumidifier() => HumidifierOn = !HumidifierOn;

        [RelayCommand]
        private void ToggleFanHeater() => FanHeaterOn = !FanHeaterOn;

        [RelayCommand]
        private void ToggleExhaust() => ExhaustOn = !ExhaustOn;

        public ManualViewModel()
        {
            // valores iniciais já definidos por campos
        }
    }
}
