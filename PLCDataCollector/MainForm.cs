using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using PLCDataCollector.Models;
using PLCDataCollector.Services;

namespace PLCDataCollector
{
    public class MainForm : Form
    {
        private readonly AppConfig _config;
        private readonly string _configPath;
        private CsvListener? _listener;
        private FileStorageService? _storage;
        private LoggingService? _logger;
        private ModbusService? _modbus;
        private ModbusStorageService? _modbusStorage;

        private Button _settingsBtn = new();
        private Button _startBtn = new();
        private Button _stopBtn = new();
        private ListBox _plcList = new();

        public MainForm(AppConfig config, string configPath)
        {
            _config = config;
            _configPath = configPath;
            Text = "PLC Data Collector";
            Width = 600; Height = 400;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _plcList.Bounds = new System.Drawing.Rectangle(10, 10, 400, 300);
            _plcList.Items.AddRange(_config.PlcIps.ToArray());
            Controls.Add(_plcList);

            _settingsBtn.Text = "Settings";
            _settingsBtn.Bounds = new System.Drawing.Rectangle(420, 10, 150, 30);
            _settingsBtn.Click += (s, e) => OpenSettings();
            Controls.Add(_settingsBtn);

            _startBtn.Text = "Start";
            _startBtn.Bounds = new System.Drawing.Rectangle(420, 50, 70, 30);
            _startBtn.Click += (s, e) => Start();
            Controls.Add(_startBtn);

            _stopBtn.Text = "Stop";
            _stopBtn.Bounds = new System.Drawing.Rectangle(500, 50, 70, 30);
            _stopBtn.Click += (s, e) => Stop();
            _stopBtn.Enabled = false;
            Controls.Add(_stopBtn);
        }

        private void OpenSettings()
        {
            using var f = new SettingsForm(_config);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                _plcList.Items.Clear();
                _plcList.Items.AddRange(_config.PlcIps.ToArray());
            }
        }

        private void Start()
        {
            _logger = new Services.LoggingService(_config);
            _listener = new CsvListener(_config.CsvListenerPort, _logger);
            _storage = new FileStorageService(_config, _logger);
            _modbus = new ModbusService(_config, _logger);
            _modbus.StatusUpdated += Modbus_StatusUpdated;
            _modbus.Start();
            _modbusStorage = new ModbusStorageService(_config, _logger);
            _listener.CsvReceived += Listener_CsvReceived;
            _listener.Start();
            _storage.Start();
            _startBtn.Enabled = false;
            _stopBtn.Enabled = true;
        }

        private void Modbus_StatusUpdated(object? sender, Services.ModbusStatusEventArgs e)
        {
            _logger?.Info($"Modbus {e.Ip} status: {(e.IsOk ? "OK" : "NG")} {e.Message}");
            if (e.Registers != null && e.IsOk)
            {
                _ = _modbusStorage?.SaveAsync(e.Ip, DateTime.Now, e.Registers);
            }
        }

        private void Listener_CsvReceived(object? sender, CsvReceivedEventArgs e)
        {
            _storage?.ReceiveCsv(e.SourceIp, e.CsvText);
        }

        private void Stop()
        {
            try { _modbus?.Dispose(); } catch { }
            try { _modbusStorage?.Dispose(); } catch { }
            try { _listener?.Dispose(); } catch { }
            try { _storage?.Dispose(); } catch { }
            _listener = null;
            _storage = null;
            _modbus = null;
            _logger = null;
            _startBtn.Enabled = true;
            _stopBtn.Enabled = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Stop();
            base.OnFormClosing(e);
        }
    }
}
