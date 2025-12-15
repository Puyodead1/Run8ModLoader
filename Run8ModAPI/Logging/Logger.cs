using System;
using System.IO;

namespace Run8ModAPI.Logging
{
    internal class Logger : ILogger
    {
        private readonly StreamWriter _writer;
        private readonly string _modName;

        public Logger(string modName, string logPath)
        {
            _modName = modName;
            _writer = new StreamWriter(logPath, false) { AutoFlush = true };
        }

        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);

        public void Error(string message, Exception ex)
        {
            Log(LogLevel.Error, $"{message}\n{ex}");
        }

        public void Log(LogLevel level, string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var levelStr = level.ToString().ToUpper().PadRight(7);
            _writer.WriteLine($"[{timestamp}] [{levelStr}] {message}");
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}