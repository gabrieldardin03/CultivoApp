using Avalonia.Controls;
using Avalonia.Input;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }

        private void OnHomeClicked(object? sender, PointerPressedEventArgs e)
        {
            _viewModel.NavigateTo("Home");
        }

        private void OnManualClicked(object? sender, PointerPressedEventArgs e)
        {
            _viewModel.NavigateTo("Manual");
        }

        private void OnCogumelosClicked(object? sender, PointerPressedEventArgs e)
        {
            _viewModel.NavigateTo("Cogumelos");
        }
    }
}
