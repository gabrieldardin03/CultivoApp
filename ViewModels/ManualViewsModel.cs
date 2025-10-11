using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CultivoApp.ViewModels
{
    public class ManualViewModel : INotifyPropertyChanged
    {
        private bool _isAutomatic = true;
        public bool IsAutomatic
        {
            get => _isAutomatic;
            set
            {
                if (_isAutomatic != value)
                {
                    _isAutomatic = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsManual));
                }
            }
        }

        public bool IsManual => !IsAutomatic;

        private bool _humidifierOn;
        public bool HumidifierOn
        {
            get => _humidifierOn;
            set { _humidifierOn = value; OnPropertyChanged(); }
        }

        private bool _fanHeaterOn;
        public bool FanHeaterOn
        {
            get => _fanHeaterOn;
            set { _fanHeaterOn = value; OnPropertyChanged(); }
        }

        private bool _exhaustOn;
        public bool ExhaustOn
        {
            get => _exhaustOn;
            set { _exhaustOn = value; OnPropertyChanged(); }
        }

        private double _temperature = 24.3;
        public double Temperature
        {
            get => _temperature;
            set { _temperature = value; OnPropertyChanged(); }
        }

        private double _humidity = 78.5;
        public double Humidity
        {
            get => _humidity;
            set { _humidity = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
