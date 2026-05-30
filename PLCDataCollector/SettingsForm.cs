using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PLCDataCollector.Models;

namespace PLCDataCollector
{
    public class SettingsForm : Form
    {
        private readonly AppConfig _config;

        private TextBox _plcIps = new TextBox();
        private NumericUpDown _port = new NumericUpDown();
        private NumericUpDown _interval = new NumericUpDown();
        private NumericUpDown _retention = new NumericUpDown();
        private TextBox _dataFolder = new TextBox();
        private Button _save = new Button();

        public SettingsForm(AppConfig config)
        {
            _config = config;
            Text = "Settings";
            Width = 500; Height = 360;
            Initialize();
        }

        private void Initialize()
        {
            var lbl1 = new Label { Text = "PLC IPs (one per line):", Left = 10, Top = 10, Width = 200 };
            _plcIps.Multiline = true; _plcIps.Left = 10; _plcIps.Top = 30; _plcIps.Width = 460; _plcIps.Height = 120;
            _plcIps.Text = string.Join("\n", _config.PlcIps);

            var lbl2 = new Label { Text = "CSV Listen Port:", Left = 10, Top = 160, Width = 120 };
            _port.Left = 140; _port.Top = 158; _port.Minimum = 1; _port.Maximum = 65535; _port.Value = _config.CsvListenerPort;

            var lbl3 = new Label { Text = "Save Interval (sec):", Left = 10, Top = 190, Width = 120 };
            _interval.Left = 140; _interval.Top = 188; _interval.Minimum = 1; _interval.Maximum = 86400; _interval.Value = _config.SaveIntervalSeconds;

            var lbl4 = new Label { Text = "Retention (years):", Left = 10, Top = 220, Width = 120 };
            _retention.Left = 140; _retention.Top = 218; _retention.Minimum = 0; _retention.Maximum = 100; _retention.Value = _config.RetentionYears;

            var lbl5 = new Label { Text = "Data folder:", Left = 10, Top = 250, Width = 120 };
            _dataFolder.Left = 140; _dataFolder.Top = 248; _dataFolder.Width = 220; _dataFolder.Text = _config.DataFolder;
            var browse = new Button { Text = "Browse", Left = 370, Top = 246, Width = 100 };
            browse.Click += (s, e) => { using var d = new FolderBrowserDialog(); if (d.ShowDialog() == DialogResult.OK) _dataFolder.Text = d.SelectedPath; };

            _save.Text = "Save"; _save.Left = 380; _save.Top = 290; _save.Click += (s, e) => SaveAndClose();

            Controls.AddRange(new Control[] { lbl1, _plcIps, lbl2, _port, lbl3, _interval, lbl4, _retention, lbl5, _dataFolder, browse, _save });
        }

        private void SaveAndClose()
        {
            _config.PlcIps = _plcIps.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
            _config.CsvListenerPort = (int)_port.Value;
            _config.SaveIntervalSeconds = (int)_interval.Value;
            _config.RetentionYears = (int)_retention.Value;
            _config.DataFolder = string.IsNullOrWhiteSpace(_dataFolder.Text) ? "Data" : _dataFolder.Text;
            if (!Directory.Exists(_config.DataFolder)) Directory.CreateDirectory(_config.DataFolder);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
