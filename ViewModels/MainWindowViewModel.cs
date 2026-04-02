using CommunityToolkit.Mvvm.ComponentModel;
using CultivoApp.Views;
using CultivoApp.Services;

namespace CultivoApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        private readonly HomeView _homeView;
        private readonly ManualView _manualView;
        private readonly CogumelosView _cogumelosView;
        private readonly HistoricoView _historicoView;

        public MainWindowViewModel()
        {
            // 🔹 Cria as views uma única vez
            _homeView = new HomeView();
            _manualView = new ManualView();
            _cogumelosView = new CogumelosView();
            _historicoView = new HistoricoView();

            // 🔹 Página inicial
            _currentView = _homeView;
        }

        public void NavigateTo(string page)
        {
            switch (page)
            {
                case "Home":
                    CurrentView = _homeView;
                    break;
                case "Manual":
                    CurrentView = _manualView;
                    break;
                case "Cogumelos":
                    CurrentView = _cogumelosView;
                    break;
                case "Historico":
                    CurrentView = _historicoView;
                    break;
            }
        }
    }
}
