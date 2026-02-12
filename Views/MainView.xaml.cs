using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MSFSGraphicsPresetSwitcher.Services;
using MSFSGraphicsPresetSwitcher.ViewModels;

namespace MSFSGraphicsPresetSwitcher.Views
{
    public sealed partial class MainView : UserControl
    {
        public MainViewModel ViewModel { get; }

        public MainView()
        {
            this.InitializeComponent();

            ViewModel = new MainViewModel(
                new GraphicsPresetService()
            );

            DataContext = ViewModel;
        }
    }
}
