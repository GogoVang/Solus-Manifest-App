using Microsoft.Toolkit.Uwp.Notifications;
using System;

namespace SolusManifestApp.Services
{
    public class NotificationService
    {
        private readonly SettingsService _settingsService;
        private static bool _isInitialized = false;

        public NotificationService(SettingsService settingsService)
        {
            _settingsService = settingsService;

            if (!_isInitialized)
            {
                ToastNotificationManagerCompat.OnActivated += toastArgs =>
                {
                    // Handle toast activation if needed
                };
                _isInitialized = true;
            }
        }

        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info)
        {
            var settings = _settingsService.LoadSettings();
            if (!settings.ShowNotifications)
                return;

            try
            {
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .Show();
            }
            catch (Exception ex)
            {
                // If toast fails, log the error but don't crash
                System.Diagnostics.Debug.WriteLine($"Toast notification failed: {ex.Message}");
            }
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            ShowNotification(title, message, NotificationType.Success);
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            ShowNotification(title, message, NotificationType.Warning);
        }

        public void ShowError(string message, string title = "Error")
        {
            ShowNotification(title, message, NotificationType.Error);
        }

        public void ShowDownloadComplete(string gameName)
        {
            ShowSuccess($"{gameName} has been downloaded successfully!", "Download Complete");
        }

        public void ShowInstallComplete(string gameName)
        {
            ShowSuccess($"{gameName} has been installed successfully! Restart Steam for changes to take effect.", "Installation Complete");
        }

        public void ShowUpdateAvailable(string version)
        {
            ShowNotification("Update Available", $"A new version ({version}) is available!", NotificationType.Info);
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
