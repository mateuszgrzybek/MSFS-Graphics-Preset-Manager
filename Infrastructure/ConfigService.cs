using System;
using System.IO;
using System.Text.Json;

namespace MSFSGraphicsPresetSwitcher.Infrastructure
{
    public class ConfigService
    {
        public static ConfigData? LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return null;

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<ConfigData>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void SaveConfig(ConfigData data)
        {
            Directory.CreateDirectory(ConfigFolder);

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        private static readonly string ConfigFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MSFSGraphicsPresets");

        private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");

    }

    public class ConfigData
    {
        public string LiveFilePath { get; set; } = "";
        public string PresetFolderPath { get; set; } = "";
    }
}
