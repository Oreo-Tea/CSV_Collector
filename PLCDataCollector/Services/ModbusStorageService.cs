using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PLCDataCollector.Models;

namespace PLCDataCollector.Services
{
    public class ModbusStorageService : IDisposable
    {
        private readonly AppConfig _config;
        private readonly LoggingService? _log;
        private readonly string _root;
        public ModbusStorageService(AppConfig config, LoggingService? log = null)
        {
            _config = config;
            _log = log;
            _root = _config.DataFolder;
            Directory.CreateDirectory(_root);
            // DB support removed per user request; only CSV persistence retained
        }

        public async Task SaveAsync(string ip, DateTime timestamp, ushort[] registers)
        {
            try
            {
                if (_config.SaveModbusToCsv)
                {
                    await SaveCsvAsync(ip, timestamp, registers).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"SaveAsync error: {ex.Message}");
            }
        }

        private async Task SaveCsvAsync(string ip, DateTime timestamp, ushort[] registers)
        {
            try
            {
                var folder = Path.Combine(_root, timestamp.ToString("yyyy-MM-dd"));
                Directory.CreateDirectory(folder);
                var filename = Path.Combine(folder, $"modbus_{ip}_{timestamp:yyyyMMdd_HHmmss}.csv");
                // Header
                var header = new StringBuilder();
                header.Append("Timestamp,Ip");
                for (int i = 0; i < registers.Length; i++) header.Append($",Reg{i}");
                header.AppendLine();
                // Row
                var row = new StringBuilder();
                row.Append(timestamp.ToString("o"));
                row.Append($"," + ip);
                for (int i = 0; i < registers.Length; i++) row.Append($"," + registers[i]);
                row.AppendLine();
                await File.WriteAllTextAsync(filename, header.ToString() + row.ToString(), Encoding.UTF8).ConfigureAwait(false);
                _log?.Info($"Wrote Modbus CSV {filename}");
            }
            catch (Exception ex)
            {
                _log?.Error($"SaveCsvAsync error: {ex.Message}");
            }
        }

        private async Task SaveDbAsync(string ip, DateTime timestamp, ushort[] registers)
        {
            // DB storage removed; this method is intentionally left blank
            await Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
