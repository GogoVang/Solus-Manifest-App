using SolusManifestApp.ViewModels;
using SolusManifestApp.Services;
using System.Windows;
using System.Windows.Input;

namespace SolusManifestApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly SettingsService _settingsService;

        public MainWindow(MainViewModel viewModel, SettingsService settingsService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _settingsService = settingsService;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;

            // Restore window size
            var settings = _settingsService.LoadSettings();
            Width = settings.WindowWidth;
            Height = settings.WindowHeight;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.CheckForUpdates();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window size
            var settings = _settingsService.LoadSettings();
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            _settingsService.SaveSettings(settings);

            // Check if we should minimize to tray instead of closing
            if (settings.MinimizeToTray)
            {
                e.Cancel = true;
                var app = Application.Current as App;
                var trayService = app?.GetTrayIconService();
                trayService?.ShowInTray();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if we should minimize to tray instead of closing
            var settings = _settingsService.LoadSettings();
            if (settings.MinimizeToTray)
            {
                var app = Application.Current as App;
                var trayService = app?.GetTrayIconService();
                trayService?.ShowInTray();
            }
            else
            {
                Close();
            }
        }
    }
}
