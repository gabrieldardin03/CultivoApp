using Avalonia.Controls;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class ManualView : UserControl
    {
        public ManualView()
        {
            InitializeComponent();
            DataContext = new ManualViewModel();
        }
    }
}
