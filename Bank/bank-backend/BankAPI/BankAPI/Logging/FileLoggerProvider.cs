using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BankAPI.Logging
{
    /// <summary>
    /// PCI DSS 10.5.1 - Logs all access to network resources and cardholder data to daily rotating files.
    /// Each log entry includes: UTC timestamp, level, correlation ID, and source category.
    /// </summary>
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logFolder;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
        private readonly object _writeLock = new();

        private StreamWriter? _writer;
        private string _currentDate = string.Empty;

        // Set by middleware per request so every log line carries the correlation ID
        public AsyncLocal<string?> CurrentScope { get; } = new AsyncLocal<string?>();

        public FileLoggerProvider(string logFolder)
        {
            _logFolder = logFolder;
            Directory.CreateDirectory(logFolder);
            EnsureCurrentFile();
        }

        private void EnsureCurrentFile()
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (_currentDate == today && _writer != null)
                return;

            _writer?.Flush();
            _writer?.Dispose();

            _currentDate = today;
            var filePath = Path.Combine(_logFolder, $"bank-{today}.log");
            var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(stream, System.Text.Encoding.UTF8) { AutoFlush = true };
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new FileLogger(name, this));

        public void WriteLog(string line)
        {
            lock (_writeLock)
            {
                try
                {
                    EnsureCurrentFile();
                    _writer?.WriteLine(line);
                }
                catch
                {
                    // If the write fails (e.g. disk full) we swallow it — logging must never crash the app
                }
            }
        }

        public void Dispose()
        {
            _writer?.Flush();
            _writer?.Dispose();
        }
    }
}
