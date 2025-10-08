using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new HomeViewModel(); // garante ViewModel em runtime
        }
    }
}
