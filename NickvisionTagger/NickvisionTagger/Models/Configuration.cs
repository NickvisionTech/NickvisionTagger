using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionTagger.Models
{
    public class Configuration
    {
        private static readonly string _configDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionTagger";
        private static readonly string _configPath = $"{_configDir}\\config.json";

        public bool IsLightTheme { get; set; }
        public string PreviousMusicFolder { get; set; }
        public bool IncludeSubfolders { get; set; }

        public Configuration()
        {
            IsLightTheme = false;
            PreviousMusicFolder = "";
            IncludeSubfolders = true;
        }

        public static async Task<Configuration> LoadAsync()
        {
            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch
            {
                return new Configuration();
            }
        }

        public async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(this);
            if (!Directory.Exists(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}
