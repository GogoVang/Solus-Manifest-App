using System;
using System.IO;

namespace SolusManifestApp.Services
{
    public class LoggerService
    {
        private static readonly object _lock = new object();
        private readonly string _logFilePath;

        public LoggerService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolusManifestApp"
            );
            Directory.CreateDirectory(appDataPath);

            // Create new log file with timestamp
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(appDataPath, $"solus_{timestamp}.log");

            Log("INFO", "Logger initialized");
            Log("INFO", $"Log file: {_logFilePath}");
        }

        public void Log(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] {message}";

                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);

                    // Also write to debug output for convenience
                    System.Diagnostics.Debug.WriteLine(logEntry);
                }
                catch
                {
                    // Silently fail if logging fails
                }
            }
        }

        public void Info(string message) => Log("INFO", message);
        public void Debug(string message) => Log("DEBUG", message);
        public void Warning(string message) => Log("WARN", message);
        public void Error(string message) => Log("ERROR", message);

        public string GetLogFilePath() => _logFilePath;

        public string GetLogsFolderPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SolusManifestApp"
            );
        }

        public void OpenLogsFolder()
        {
            var logsFolderPath = GetLogsFolderPath();
            if (Directory.Exists(logsFolderPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", logsFolderPath);
            }
        }

        public void ClearOldLogs()
        {
            try
            {
                var logsFolderPath = GetLogsFolderPath();
                if (!Directory.Exists(logsFolderPath))
                {
                    return;
                }

                var logFiles = Directory.GetFiles(logsFolderPath, "solus_*.log");
                foreach (var logFile in logFiles)
                {
                    // Don't delete current log file
                    if (logFile != _logFilePath)
                    {
                        try
                        {
                            File.Delete(logFile);
                            Info($"Deleted old log file: {Path.GetFileName(logFile)}");
                        }
                        catch (Exception ex)
                        {
                            Error($"Failed to delete log file {Path.GetFileName(logFile)}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to clear old logs: {ex.Message}");
            }
        }
    }
}
