using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PLCDataCollector.Models;

namespace PLCDataCollector;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var cfgPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        AppConfig config;
        if (File.Exists(cfgPath))
        {
            var json = File.ReadAllText(cfgPath);
            config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        else
        {
            config = new AppConfig();
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(cfgPath, json);
        }

        Application.Run(new MainForm(config, cfgPath));
    }
}
