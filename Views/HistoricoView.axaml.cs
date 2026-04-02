// HistoricoView.axaml.cs
using Avalonia.Controls;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class HistoricoView : UserControl
    {
        public HistoricoView()
        {
            InitializeComponent();
            
            // ✅ CORREÇÃO:
            // Define o DataContext para a ViewModel correta (a que tem os gráficos),
            // em vez de usar a HomeViewModelInstance.
            DataContext = new HistoricoViewModel();
        }
    }
}