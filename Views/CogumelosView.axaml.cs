using Avalonia.Controls;
using CultivoApp.ViewModels;

namespace CultivoApp.Views
{
    public partial class CogumelosView : UserControl
    {
        public CogumelosView()
        {
            // O DataContext DEVE vir antes do InitializeComponent para os Bindings não quebrarem!
            DataContext = new CogumelosViewModel();
            InitializeComponent();
        }
    }
}