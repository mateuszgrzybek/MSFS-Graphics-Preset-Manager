using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MSFSGraphicsPresetManager.Infrastructure;
using MSFSGraphicsPresetManager.Models;
using MSFSGraphicsPresetManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI;
using WinRT.Interop;

namespace MSFSGraphicsPresetManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public enum StatusType
        {
            Info,
            Success,
            Error
        }

        public ObservableCollection<GraphicsPresetEntry> Presets { get; } = new();
        
        private GraphicsPresetEntry? _selectedPreset;
        private CancellationTokenSource? _statusCts;
        private SolidColorBrush _statusBrush = new SolidColorBrush(Colors.White);
        private readonly GraphicsPresetService _service;
        private string _liveFilePath = "";
        private string _presetFolderPath = "";
        private string _newPresetName = "";
        private string _statusMessage = "";
        private double _statusOpacity;
        private int _statusVersion = 0;

        public GraphicsPresetEntry? SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset == value) return;
                _selectedPreset = value;
                OnPropertyChanged();
                ActivatePresetCommand.NotifyCanExecuteChanged();
                RemovePresetCommand.NotifyCanExecuteChanged();
            }
        }

        public string LiveFilePath
        {
            get => _liveFilePath;
            set { _liveFilePath = value; OnPropertyChanged(); }
        }

        public string PresetFolderPath
        {
            get => _presetFolderPath;
            set { _presetFolderPath = value; OnPropertyChanged(); }
        }

        public string NewPresetName
        {
            get => _newPresetName;
            set
            {
                if (_newPresetName == value) return;
                _newPresetName = value;
                OnPropertyChanged();
                SavePresetCommand.NotifyCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public double StatusOpacity
        {
            get => _statusOpacity;
            set { _statusOpacity = value; OnPropertyChanged(); }
        }

        public SolidColorBrush StatusBrush
        {
            get => _statusBrush;
            set { _statusBrush = value; OnPropertyChanged(); }
        }

        public RelayCommand SavePresetCommand { get; }
        public RelayCommand ActivatePresetCommand { get; }
        public RelayCommand RemovePresetCommand { get; }
        public RelayCommand BrowseLiveFileCommand { get; }
        public RelayCommand BrowsePresetFolderCommand { get; }

        public MainViewModel(GraphicsPresetService service)
        {
            _service = service;

            SavePresetCommand = new RelayCommand(
                SavePreset,
                () => CanSavePreset
            );

            ActivatePresetCommand = new RelayCommand(
                ActivatePreset,
                () => IsPresetSelected
            );

            RemovePresetCommand = new RelayCommand(
                RemovePreset,
                () => IsPresetSelected
            );

            BrowseLiveFileCommand = new RelayCommand(BrowseLiveFile);

            BrowsePresetFolderCommand = new RelayCommand(BrowsePresetFolder);

            LoadPaths();
            RefreshPresets();
        }

        public bool CanSavePreset =>
            !string.IsNullOrWhiteSpace(NewPresetName);

        public bool IsPresetSelected =>
            SelectedPreset != null;

        private void LoadPaths()
        {
            var config = ConfigService.LoadConfig();

            if (config != null)
            {
                LiveFilePath = config.LiveFilePath;
                PresetFolderPath = config.PresetFolderPath;
            }
            else
            {
                LiveFilePath = FilePathHelper.AutoDetectUserCfgPath() ?? "";
                PresetFolderPath = FilePathHelper.AutoDetectPresetFolder() ?? "";

                ConfigService.SaveConfig(new ConfigData
                {
                    LiveFilePath = LiveFilePath,
                    PresetFolderPath = PresetFolderPath
                });
            }
        }

        private nint GetWindowHandle()
        {
            nint hWnd = WindowNative.GetWindowHandle(App.Window);
            return hWnd;
        }

        private async void BrowseLiveFile()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".opt");

            InitializeWithWindow.Initialize(picker, GetWindowHandle());

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                LiveFilePath = file.Path;

                ConfigService.SaveConfig(new ConfigData
                {
                    LiveFilePath = LiveFilePath,
                    PresetFolderPath = PresetFolderPath
                });
            }
        }

        private async void BrowsePresetFolder()
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");

            InitializeWithWindow.Initialize(picker, GetWindowHandle());

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                PresetFolderPath = folder.Path;

                ConfigService.SaveConfig(new ConfigData
                {
                    LiveFilePath = LiveFilePath,
                    PresetFolderPath = PresetFolderPath
                });
            }
        }

        private void RefreshPresets()
        {
            Presets.Clear();
            foreach (var preset in _service.ListPresets(PresetFolderPath))
                Presets.Add(preset);
        }

        private void SavePreset()
        {
            if (string.IsNullOrWhiteSpace(NewPresetName)) return;

            _service.SavePreset(
                LiveFilePath,
                PresetFolderPath,
                NewPresetName.Trim()
            );

            NewPresetName = "";
            RefreshPresets();
            ShowStatus("Preset saved", StatusType.Info);
        }

        private void ActivatePreset()
        {
            if (SelectedPreset == null) return;

            _service.ActivatePreset(
                SelectedPreset,
                LiveFilePath,
                PresetFolderPath
            );

            ShowStatus($"Preset '{SelectedPreset.Name}' activated", StatusType.Success);
        }

        private void RemovePreset()
        {
            if (SelectedPreset == null) return;

            var RemovedPresetName = SelectedPreset.Name;

            _service.RemovePreset(SelectedPreset, PresetFolderPath);
            RefreshPresets();
            ShowStatus($"Preset '{RemovedPresetName}' removed", StatusType.Error);
        }

        private void ShowStatus(string message, StatusType type = StatusType.Info)
        {
            StatusMessage = message;
            StatusOpacity = 1;

            int currentVersion = ++_statusVersion;

            switch (type)
            {
                case StatusType.Success:
                    StatusBrush = new SolidColorBrush(Colors.Green);
                    break;
                case StatusType.Error:
                    StatusBrush = new SolidColorBrush(Colors.IndianRed);
                    break;
                case StatusType.Info:
                default:
                    StatusBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                    break;
            }

            _ = HideStatusAsync(currentVersion);
        }

        private async Task HideStatusAsync(int version)
        {
            try
            {
                await Task.Delay(2000);

                _ = App.Window.DispatcherQueue.TryEnqueue(async () =>
                {
                    if (version != _statusVersion)
                        return;

                    double opacity = StatusOpacity;
                    const double duration = 0.5;
                    const int steps = 20;
                    double stepTime = duration / steps;

                    for (int i = 0; i < steps; i++)
                    {
                        opacity -= 1.0 / steps;
                        if (opacity < 0) opacity = 0;
                        StatusOpacity = opacity;
                        await Task.Delay(TimeSpan.FromSeconds(stepTime));
                    }

                    StatusOpacity = 0;
                });
            }
            catch (TaskCanceledException) {}
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
