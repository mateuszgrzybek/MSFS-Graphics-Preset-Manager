using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;

namespace MSFSGraphicsPresetSwitcher
{
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();

            if (AppWindowTitleBar.IsCustomizationSupported() is true)
            {
                nint hWnd = WindowNative.GetWindowHandle(this);
                WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(wndId);
                appWindow.Title = "MSFS Graphics Preset Manager";
                appWindow.SetTaskbarIcon(@"Assets\Main.ico");
                appWindow.SetTitleBarIcon(@"Assets\Main.ico");
            }
        }
    }
}
