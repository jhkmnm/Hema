using System;
using System.IO;
using System.Text.Json;

namespace Client.Services
{
    public class AppConfigService
    {
        private const string ConfigFileName = "config.json";
        private AppConfig _config;

        public class AppConfig
        {
            public string SetupFilesPath { get; set; } = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "SoftwareManager", "Setups");
        }

        public AppConfigService()
        {
            LoadConfig();
            EnsureSetupDirectory();
        }

        public string SetupFilesPath => _config.SetupFilesPath;

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    var json = File.ReadAllText(ConfigFileName);
                    _config = JsonSerializer.Deserialize<AppConfig>(json);
                }
                else
                {
                    _config = new AppConfig();
                    SaveConfig();
                }
            }
            catch
            {
                _config = new AppConfig();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFileName, json);
        }

        private void EnsureSetupDirectory()
        {
            if (!Directory.Exists(_config.SetupFilesPath))
            {
                Directory.CreateDirectory(_config.SetupFilesPath);
            }
        }
    }
} 
