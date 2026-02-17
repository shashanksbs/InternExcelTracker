
using System;

namespace InternExcelTracker.Api.Services
{
    public class ConsoleLoggerService : ILoggerService
    {
        public void Log(string message)
        {
            var logMessage = $"[{DateTime.UtcNow}] {message}";
            Console.WriteLine(logMessage);
        }
    }
}
