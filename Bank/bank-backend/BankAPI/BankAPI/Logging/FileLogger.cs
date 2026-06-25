using Microsoft.Extensions.Logging;

namespace BankAPI.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly FileLoggerProvider _provider;

        public FileLogger(string categoryName, FileLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var correlationId = _provider.CurrentScope.Value ?? "-";
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = logLevel switch
            {
                LogLevel.Information => "INFO",
                LogLevel.Warning     => "WARN",
                LogLevel.Error       => "ERRO",
                LogLevel.Critical    => "CRIT",
                _                    => logLevel.ToString().ToUpper()[..4]
            };

            // Shorten category: BankAPI.Services.PaymentService -> PaymentService
            var shortCategory = _categoryName.Contains('.')
                ? _categoryName[((_categoryName.LastIndexOf('.') + 1))..]
                : _categoryName;

            var message = formatter(state, exception);
            var line = $"[{timestamp}] [{level}] [corrId={correlationId}] [{shortCategory}] {message}";

            if (exception != null)
                line += $"{Environment.NewLine}  EXCEPTION: {exception}";

            _provider.WriteLog(line);
        }
    }
}
