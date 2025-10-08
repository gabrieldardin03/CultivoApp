using Avalonia.Controls;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            DataContext = new HomeViewModel();
        }
    }
}
