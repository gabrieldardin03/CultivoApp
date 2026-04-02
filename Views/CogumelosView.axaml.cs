using Avalonia.Controls;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class CogumelosView : UserControl
    {
        public CogumelosView()
        {
            InitializeComponent();
            DataContext = new CogumelosViewModel();
        }
    }
}
