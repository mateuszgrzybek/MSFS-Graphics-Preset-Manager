using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MSFSGraphicsPresetManager.Services;
using MSFSGraphicsPresetManager.ViewModels;

namespace MSFSGraphicsPresetManager.Views
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
