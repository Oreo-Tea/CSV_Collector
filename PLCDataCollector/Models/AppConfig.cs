using System.Collections.Generic;

namespace PLCDataCollector.Models
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warn = 2,
        Error = 3
    }

    public class AppConfig
    {
        public List<string> PlcIps { get; set; } = new List<string> { "192.168.0.10" };
        public int CsvListenerPort { get; set; } = 1502;
        public int SaveIntervalSeconds { get; set; } = 60;
        public int RetentionYears { get; set; } = 10;
        public string DataFolder { get; set; } = "Data";

        // Logging
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public int LogRetentionDays { get; set; } = 30;
        public long LogMaxFileSizeBytes { get; set; } = 5_000_000; // 5 MB

        // Modbus read settings
        public byte ModbusSlaveId { get; set; } = 1;
        public int ModbusReadStart { get; set; } = 0;
        public int ModbusReadCount { get; set; } = 10;
        public int ModbusPollIntervalSeconds { get; set; } = 10;
        // Modbus storage options
        public bool SaveModbusToCsv { get; set; } = true;
    }
}
