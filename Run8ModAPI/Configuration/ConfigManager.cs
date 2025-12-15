using Newtonsoft.Json;
using System.IO;

namespace Run8ModAPI.Configuration
{
    internal class ConfigManager : IConfigManager
    {
        private readonly string _configPath;

        public ConfigManager(string modDirectory)
        {
            _configPath = Path.Combine(modDirectory, "config.json");
        }

        public T GetConfig<T>() where T : class, new()
        {
            if (!File.Exists(_configPath))
            {
                var defaultConfig = new T();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var json = File.ReadAllText(_configPath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void SaveConfig<T>(T config) where T : class
        {
            var json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_configPath, json);
        }
    }
}