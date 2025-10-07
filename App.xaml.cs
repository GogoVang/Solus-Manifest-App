using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolusManifestApp.Helpers;
using SolusManifestApp.Services;
using SolusManifestApp.ViewModels;
using SolusManifestApp.Views;
using System;
using System.Linq;
using System.Windows;

namespace SolusManifestApp
{
    public partial class App : Application
    {
        private readonly IHost _host;
        private SingleInstanceHelper? _singleInstance;
        private TrayIconService? _trayIconService;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Services
                    services.AddSingleton<LoggerService>();
                    services.AddSingleton<SettingsService>();
                    services.AddSingleton<SteamService>();
                    services.AddSingleton<SteamGamesService>();
                    services.AddSingleton<SteamApiService>();
                    services.AddSingleton<ManifestApiService>();
                    services.AddSingleton<DownloadService>();
                    services.AddSingleton<FileInstallService>();
                    services.AddSingleton<UpdateService>();
                    services.AddSingleton<NotificationService>();
                    services.AddSingleton<CacheService>();
                    services.AddSingleton<BackupService>();
                    services.AddSingleton<DepotDownloadService>();
                    services.AddSingleton<SteamLibraryService>();
                    services.AddSingleton<ThemeService>();
                    services.AddSingleton<ProtocolHandlerService>();

                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<HomeViewModel>();
                    services.AddTransient<LuaInstallerViewModel>();
                    services.AddTransient<LibraryViewModel>();
                    services.AddTransient<StoreViewModel>();
                    services.AddTransient<DownloadsViewModel>();
                    services.AddSingleton<ToolsViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<SupportViewModel>();

                    // Views
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Register protocol if not already registered
            if (!ProtocolRegistrationHelper.IsProtocolRegistered())
            {
                ProtocolRegistrationHelper.RegisterProtocol();
            }

            // Check for single instance
            _singleInstance = new SingleInstanceHelper();
            if (!_singleInstance.TryAcquire())
            {
                // Not the first instance, send args to first instance and exit
                var args = string.Join(" ", e.Args);
                if (!string.IsNullOrEmpty(args))
                {
                    SingleInstanceHelper.SendArgumentsToFirstInstance(args);
                }
                Shutdown();
                return;
            }

            // This is the first instance, set up IPC listener
            _singleInstance.ArgumentsReceived += async (sender, args) =>
            {
                await Dispatcher.InvokeAsync(() => HandleProtocolUrl(args));
            };

            await _host.StartAsync();

            // Load and apply theme
            var settingsService = _host.Services.GetRequiredService<SettingsService>();
            var themeService = _host.Services.GetRequiredService<ThemeService>();
            var settings = settingsService.LoadSettings();
            themeService.ApplyTheme(settings.Theme);

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            // Initialize tray icon service
            _trayIconService = new TrayIconService(mainWindow, settingsService);
            _trayIconService.Initialize();

            mainWindow.Show();

            // Handle protocol URL if passed as argument
            if (e.Args.Length > 0)
            {
                HandleProtocolUrl(string.Join(" ", e.Args));
            }

            base.OnStartup(e);
        }

        private async void HandleProtocolUrl(string url)
        {
            var protocolPath = ProtocolRegistrationHelper.ParseProtocolUrl(url);
            if (!string.IsNullOrEmpty(protocolPath))
            {
                var protocolHandler = _host.Services.GetRequiredService<ProtocolHandlerService>();
                await protocolHandler.HandleProtocolAsync(protocolPath);
            }
        }

        public TrayIconService? GetTrayIconService()
        {
            return _trayIconService;
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _trayIconService?.Dispose();
            _singleInstance?.Dispose();

            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }
}
