using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PLCDataCollector.Models;
using Modbus.Device;

namespace PLCDataCollector.Services
{
    public class ModbusStatusEventArgs : EventArgs
    {
        public string Ip { get; set; } = "";
        public bool IsOk { get; set; }
        public string Message { get; set; } = "";
        public ushort[]? Registers { get; set; }
    }

    public class ModbusService : IDisposable
    {
        private readonly AppConfig _config;
        private readonly LoggingService? _log;
        private CancellationTokenSource? _cts;
        private Task? _loopTask;

        public event EventHandler<ModbusStatusEventArgs>? StatusUpdated;

        public ModbusService(AppConfig config, LoggingService? log = null)
        {
            _config = config;
            _log = log;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
            _log?.Info("ModbusService started");
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            _log?.Info("ModbusService stopping");
        }

        private async Task LoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var ip in _config.PlcIps)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        using var tcp = new TcpClient();
                        var connectTask = tcp.ConnectAsync(ip, 502);
                        var timeout = Task.Delay(3000, token);
                        var completed = await Task.WhenAny(connectTask, timeout).ConfigureAwait(false);
                        if (completed != connectTask || !tcp.Connected)
                        {
                            StatusUpdated?.Invoke(this, new ModbusStatusEventArgs { Ip = ip, IsOk = false, Message = "connect timeout" });
                            _log?.Warn($"Modbus connect timeout: {ip}");
                            continue;
                        }
                        _log?.Info($"Modbus connected: {ip}");

                        try
                        {
                            // Use ModbusIpMaster to read holding registers
                            using var master = ModbusIpMaster.CreateIp(tcp);
                            var start = (ushort)_config.ModbusReadStart;
                            var count = (ushort)_config.ModbusReadCount;
                            byte slaveId = _config.ModbusSlaveId;
                            var registers = master.ReadHoldingRegisters(slaveId, start, count);
                            StatusUpdated?.Invoke(this, new ModbusStatusEventArgs { Ip = ip, IsOk = true, Message = "read ok", Registers = registers });
                            _log?.Info($"Modbus read from {ip}: start={start} count={count} => {string.Join(",", registers)}");
                        }
                        catch (Exception ex)
                        {
                            StatusUpdated?.Invoke(this, new ModbusStatusEventArgs { Ip = ip, IsOk = false, Message = ex.Message });
                            _log?.Error($"Modbus read error {ip}: {ex.Message}");
                        }
                        finally
                        {
                            try { tcp.Close(); } catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusUpdated?.Invoke(this, new ModbusStatusEventArgs { Ip = ip, IsOk = false, Message = ex.Message });
                        _log?.Error($"Modbus error {ip}: {ex.Message}");
                    }
                }
                await Task.Delay(Math.Max(1000, _config.ModbusPollIntervalSeconds * 1000), token).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
