using System;
using System.IO;
using System.Linq;
using System.Text;
using PLCDataCollector.Models;

namespace PLCDataCollector.Services
{
    public class LoggingService
    {
        private readonly string _logFolder;
        private readonly object _lock = new object();
        private readonly AppConfig _config;

        public LoggingService(AppConfig config)
        {
            _config = config;
            _logFolder = Path.Combine(_config.DataFolder, "Logs");
            Directory.CreateDirectory(_logFolder);
        }

        private string CurrentLogPath => Path.Combine(_logFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".log");

        private bool ShouldLog(LogLevel level)
        {
            return level >= _config.LogLevel;
        }

        public void Debug(string message) => Write(LogLevel.Debug, "DEBUG", message);
        public void Info(string message) => Write(LogLevel.Info, "INFO", message);
        public void Warn(string message) => Write(LogLevel.Warn, "WARN", message);
        public void Error(string message) => Write(LogLevel.Error, "ERROR", message);

        private void Write(LogLevel levelEnum, string levelText, string message)
        {
            if (!ShouldLog(levelEnum)) return;
            try
            {
                RotateIfNeeded();
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {levelText} {message}" + Environment.NewLine;
                lock (_lock)
                {
                    File.AppendAllText(CurrentLogPath, line, Encoding.UTF8);
                }
                CleanupOldLogs();
            }
            catch { }
        }

        private void RotateIfNeeded()
        {
            try
            {
                var path = CurrentLogPath;
                if (!File.Exists(path)) return;
                var fi = new FileInfo(path);
                if (fi.Length <= _config.LogMaxFileSizeBytes) return;
                // rotate: move existing to .1, .2 ... keep up to 5
                var max = 5;
                for (int i = max - 1; i >= 1; i--)
                {
                    var src = path + "." + i;
                    var dst = path + "." + (i + 1);
                    if (File.Exists(src))
                    {
                        if (File.Exists(dst)) File.Delete(dst);
                        File.Move(src, dst);
                    }
                }
                var first = path + ".1";
                if (File.Exists(first)) File.Delete(first);
                File.Move(path, first);
            }
            catch { }
        }

        private void CleanupOldLogs()
        {
            try
            {
                if (_config.LogRetentionDays <= 0) return;
                var files = Directory.GetFiles(_logFolder, "*.log").Concat(Directory.GetFiles(_logFolder, "*.log.*"));
                foreach (var f in files)
                {
                    try
                    {
                        var fi = new FileInfo(f);
                        if (fi.CreationTime < DateTime.Now.AddDays(-_config.LogRetentionDays))
                        {
                            fi.Delete();
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
