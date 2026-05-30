using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using PLCDataCollector.Models;
using Timer = System.Threading.Timer;

namespace PLCDataCollector.Services
{
    public class FileStorageService : IDisposable
    {
        private readonly AppConfig _config;
        private readonly ConcurrentDictionary<string, string> _latestCsv = new();
        private Timer? _timer;
        private readonly LoggingService? _log;

        public FileStorageService(AppConfig config, LoggingService? log = null)
        {
            _config = config;
            _log = log;
        }

        public void Start()
        {
            var interval = Math.Max(1, _config.SaveIntervalSeconds) * 1000;
            _timer = new Timer(async _ => await FlushAsync(), null, interval, interval);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
        }

        public void ReceiveCsv(string sourceIp, string csv)
        {
            _latestCsv[sourceIp] = csv;
        }

        private async Task FlushAsync()
        {
            foreach (var kv in _latestCsv)
            {
                try
                {
                    var date = DateTime.Now;
                    var folder = Path.Combine(_config.DataFolder, date.ToString("yyyy-MM-dd"));
                    Directory.CreateDirectory(folder);
                    var filename = Path.Combine(folder, $"{kv.Key}_{date:yyyyMMdd_HHmmss}.csv");
                    await File.WriteAllTextAsync(filename, kv.Value).ConfigureAwait(false);
                    _log?.Info($"Wrote CSV file {filename}");
                }
                catch (Exception) { }
            }
            try
            {
                CleanupOldData();
            }
            catch { }
        }

        private void CleanupOldData()
        {
            if (_config.RetentionYears <= 0) return;
            var root = _config.DataFolder;
            if (!Directory.Exists(root)) return;
            foreach (var dir in Directory.GetDirectories(root))
            {
                var name = Path.GetFileName(dir);
                if (DateTime.TryParse(name, out var dt))
                {
                    if (dt < DateTime.Now.AddYears(-_config.RetentionYears))
                    {
                        try { Directory.Delete(dir, true); } catch { }
                        _log?.Info($"Deleted old data folder {dir}");
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
