using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CultivoApp.Views;
using CultivoApp.ViewModels; // ✅ Adicionado

namespace CultivoApp
{
    public partial class App : Application
    {
        // ✅ Instância global única do HomeViewModel
        public static HomeViewModel HomeViewModelInstance { get; private set; } = new();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
