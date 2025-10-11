using CommunityToolkit.Mvvm.ComponentModel;
using CultivoApp.Views;

namespace CultivoApp.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private object _currentView;

        public MainWindowViewModel()
        {
            // Página inicial
            _currentView = new HomeView();
        }

        public void NavigateTo(string page)
        {
            switch (page)
            {
                case "Home":
                    CurrentView = new HomeView();
                    break;
                case "Manual":
                    CurrentView = new ManualView();
                    break;
              case "Cogumelos":
                    CurrentView = new CogumelosView();
                    break;
            }
        }
    }
}
