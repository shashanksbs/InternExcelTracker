using System;
using System.IO;

namespace InternExcelTracker.Api.Services
{
    public class FileLoggerService : ILoggerService
    {
        private readonly string _logFilePath;

        public FileLoggerService(IWebHostEnvironment env)
        {
            string logDirectory = Path.Combine(env.ContentRootPath, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logFilePath = Path.Combine(logDirectory, "activity_log.txt");
        }

        public void Log(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            try
            {
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Fallback or ignore if logging fails to prevent app crash
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
